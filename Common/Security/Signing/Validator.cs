using Common.Utilities;
using System;
using System.Linq;
using System.Reflection;

namespace Common.Security.Signing
{
    public class Validator<TSigned, TUnsigned>
        where TSigned : TUnsigned, ISignable, new()
    {
        // ReSharper disable once InconsistentNaming
        private readonly Func<TSigned, byte[]> _deriveIDFromThing;
        private readonly Delegates.GenerateCredentialDelegate _generateCredentials;
        private readonly Func<TSigned, bool> _passesDomainSpecificValidation;
        private readonly Signer<TSigned, TUnsigned> _signer;
        private readonly ReplayDetector<TSigned> _replayDetector;

        public Validator(
            Delegates.GenerateCredentialDelegate generateCredentials,
            Signer<TSigned, TUnsigned> signer,
            Func<TSigned, bool> passesDomainSpecificValidation,
            // ReSharper disable once InconsistentNaming
            Func<TSigned, byte[]> deriveIDFromThing,
            ReplayDetector<TSigned> replayDetector
            )
        {
            _deriveIDFromThing = deriveIDFromThing;
            _generateCredentials = generateCredentials;
            _passesDomainSpecificValidation = passesDomainSpecificValidation;
            _replayDetector = replayDetector;
            _signer = signer;
        }

        /// <summary>
        /// Determines if the given message from a client is valid
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public bool IsValid(TSigned thing)
        {
            try
            {
                if (ReferenceEquals(thing, null))
                    return false; // The message itself is null

                // Make sure it's a full (non-null) message
                if (typeof(TSigned).GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(property => ReferenceEquals(property.GetValue(thing), null)))
                    return false; // One of the properties is null

                // Make sure we haven't seen this thing before
                if (!_replayDetector.IsNew(thing))
                    return false;

                // See if it fails domain-specific validation
                if (!_passesDomainSpecificValidation(thing))
                    return false; // It failed domain-specific validation
                
                // Derive the client's credentials from the ID they gave in the message
                var purportedCredential = _generateCredentials(_deriveIDFromThing(thing));

                // Go through the process of signing the given message using the generated client secret
                var signed = _signer.Sign(thing, purportedCredential.Secret);

                // Now see if the HMAC (using the given credential's secret) matches the reported HMAC
                return signed.HMAC.SequenceEqual(thing.HMAC);
            }
            catch
            {
                return false; // Something went wrong
            }
        }
    }
}