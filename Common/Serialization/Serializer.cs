using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Common.Serialization
{
    /// <summary>
    /// Follows a series of steps to serialize an instance of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Serializer<T> : ISerializer<T>
    {
        /// <summary>
        /// The serialization steps to follow
        /// </summary>
        private readonly LinkedList<Action<T, BinaryWriter>> _serializationSteps;

        /// <summary>
        /// Sets up a new serializer, which follows a series of steps to serialize an instance of T
        /// </summary>
        public Serializer()
        {
            _serializationSteps = new LinkedList<Action<T, BinaryWriter>>();
        }
        
        /// <summary>
        /// Enqueues a serialization step
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="converter"></param>
        public void EnqueueStep<TMember>(Func<T, TMember> converter)
        {
            // Find the write method that will handle TMember
            var writeMethodInfo = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), new[] { typeof(TMember) });
            if (writeMethodInfo == null) throw new Exception("Cannot serialized type " + typeof(TMember).Name + ". Please try a type that you can write to a BinaryWriter");
            var writeMethod = new Action<TMember, BinaryWriter>((member, writer) =>
            {
                try
                {
                    writeMethodInfo.Invoke(writer, new object[] {member});
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            });
            
            // Add a serialization step to write the converted thing into the binary writer
            _serializationSteps.AddLast((thing, writer) =>
            {
                var converted = converter(thing);
                writeMethod(converted, writer);
            });
        }

        /// <summary>
        /// Creates a serializer that performs no translation on a single value
        /// </summary>
        /// <returns></returns>
        public static ISerializer<T> CreatePassthroughSerializer()
        {
            var serializer = new Serializer<T>();
            serializer.EnqueueStep(x => x);
            return serializer;
        }

        /// <summary>
        /// Serializes the given thing using the steps that were previously enqueued
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public async Task<byte[]> SerializeAsync(T thing)
        {
            return await Task.Run(() =>
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        foreach (var step in _serializationSteps)
                        {
                            step(thing, writer);
                        }
                        writer.Flush();
                        var serialized = stream.ToArray();
                        return serialized;
                    }
                }
            });
        }
    }
}