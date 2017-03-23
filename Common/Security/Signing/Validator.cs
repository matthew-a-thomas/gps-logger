using Common.Utilities;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
// ReSharper disable ClassNeverInstantiated.Global

namespace Common.Security.Signing
{
    public class Validator<TSigned, TUnsigned>
        where TSigned : TUnsigned, ISignable, new()
    {
        // ReSharper disable once InconsistentNaming
        public delegate Task<byte[]> DeriveIDFromThingDelegateAsync(TSigned thing);
        public delegate Task<bool> PassesDomainSpecificValidationDelegateAsync(TSigned thing);

        // ReSharper disable once InconsistentNaming
        private readonly DeriveIDFromThingDelegateAsync _deriveIdFromThingAsync;
        private readonly Delegates.GenerateCredentialDelegateAsync _generateCredentialsAsync;
        private readonly PassesDomainSpecificValidationDelegateAsync _passesDomainSpecificValidationAsync;
        private readonly Signer<TSigned, TUnsigned> _signer;
        private readonly ReplayDetector<TSigned> _replayDetector;

        public Validator(
            Delegates.GenerateCredentialDelegateAsync generateCredentialsAsync,
            Signer<TSigned, TUnsigned> signer,
            PassesDomainSpecificValidationDelegateAsync passesDomainSpecificValidationAsync,
            // ReSharper disable once InconsistentNaming
            DeriveIDFromThingDelegateAsync deriveIDFromThingAsync,
            ReplayDetector<TSigned> replayDetector
            )
        {
            _deriveIdFromThingAsync = deriveIDFromThingAsync;
            _generateCredentialsAsync = generateCredentialsAsync;
            _passesDomainSpecificValidationAsync = passesDomainSpecificValidationAsync;
            _replayDetector = replayDetector;
            _signer = signer;
        }

        /// <summary>
        /// Determines if the given message from a client is valid
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public async Task<bool> IsValidAsync(TSigned thing)
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
                if (!await _passesDomainSpecificValidationAsync(thing))
                    return false; // It failed domain-specific validation
                
                // Derive the client's credentials from the ID they gave in the message
                var purportedCredential = await _generateCredentialsAsync(await _deriveIdFromThingAsync(thing));

                // Go through the process of signing the given message using the generated client secret
                var signed = await _signer.SignAsync(thing, purportedCredential.Secret);

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