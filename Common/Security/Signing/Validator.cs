using Common.Utilities;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Messages;

// ReSharper disable ClassNeverInstantiated.Global

namespace Common.Security.Signing
{
    public class Validator<T>
    {
        // ReSharper disable once InconsistentNaming
        public delegate Task<byte[]> DeriveIDFromThingDelegateAsync(Message<T> thing);
        public delegate Task<bool> PassesDomainSpecificValidationDelegateAsync(Message<T> thing);

        // ReSharper disable once InconsistentNaming
        private readonly DeriveIDFromThingDelegateAsync _deriveIdFromThingAsync;
        private readonly Delegates.GenerateCredentialDelegateAsync _generateCredentialsAsync;
        private readonly PassesDomainSpecificValidationDelegateAsync _passesDomainSpecificValidationAsync;
        private readonly Signer<T> _signer;
        private readonly ReplayDetector<Message<T>> _replayDetector;

        public Validator(
            Delegates.GenerateCredentialDelegateAsync generateCredentialsAsync,
            Signer<T> signer,
            PassesDomainSpecificValidationDelegateAsync passesDomainSpecificValidationAsync,
            // ReSharper disable once InconsistentNaming
            DeriveIDFromThingDelegateAsync deriveIDFromThingAsync,
            ReplayDetector<Message<T>> replayDetector
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
        public async Task<bool> IsValidAsync(SignedMessage<T> thing)
        {
            try
            {
                if (ReferenceEquals(thing, null))
                    return false; // The signed container is null
                if (ReferenceEquals(thing.Message, null))
                    return false; // The message is null

                // Make sure it's a full (non-null) message
                if (typeof(Message<T>).GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(property => ReferenceEquals(property.GetValue(thing.Message), null)))
                    return false; // One of the properties is null

                // Derive the client's credentials from the ID they gave in the message
                var purportedCredential = await _generateCredentialsAsync(await _deriveIdFromThingAsync(thing.Message));

                // Go through the process of signing the given message using the generated client secret
                var signed = await _signer.SignAsync(thing.Message, purportedCredential.Secret);

                // Now see if the HMAC (using the given credential's secret) matches the reported HMAC
                if (!signed.HMAC.SequenceEqual(thing.HMAC))
                    return false;

                // Make sure we haven't seen this thing before
                if (!_replayDetector.IsNew(thing.Message))
                    return false;

                // See if it fails domain-specific validation
                if (!await _passesDomainSpecificValidationAsync(thing.Message))
                    return false; // It failed domain-specific validation

                // It has passed all validations
                return true;
            }
            catch
            {
                return false; // Something went wrong
            }
        }
    }
}