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
        private readonly LinkedList<Func<T, BinaryWriter, Task>> _asyncSerializationSteps;

        /// <summary>
        /// Sets up a new serializer, which follows a series of steps to serialize an instance of T
        /// </summary>
        public Serializer()
        {
            _asyncSerializationSteps = new LinkedList<Func<T, BinaryWriter, Task>>();
        }
        
        /// <summary>
        /// Enqueues a serialization step
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="converterAsync"></param>
        public void EnqueueStepAsync<TMember>(Func<T, Task<TMember>> converterAsync)
        {
            // Find the write method that will handle TMember
            var writeMethodInfo = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), new[] { typeof(TMember) });
            if (writeMethodInfo == null) throw new Exception("Cannot serialize type " + typeof(TMember).Name + ". Please try a type that you can write to a BinaryWriter");
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
            _asyncSerializationSteps.AddLast(async (thing, writer) =>
            {
                var converted = await converterAsync(thing);
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
            serializer.EnqueueStepAsync(Task.FromResult);
            return serializer;
        }

        /// <summary>
        /// Serializes the given thing using the steps that were previously enqueued
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public async Task<byte[]> SerializeAsync(T thing)
        {
            return await Task.Run(async () =>
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        foreach (var stepAsync in _asyncSerializationSteps)
                        {
                            await stepAsync(thing, writer);
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