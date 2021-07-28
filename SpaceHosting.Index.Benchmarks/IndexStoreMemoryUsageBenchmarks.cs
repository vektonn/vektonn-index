using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using MoreLinq;
using Perfolizer.Horology;
using ProtoBuf;
using ProtoBuf.Meta;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Index.Benchmarks
{
    [Config(typeof(JobsConfig))]
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    public class IndexStoreMemoryUsageBenchmarks
    {
        private const int VectorDimension = 32;

        [Params(50_000, 500_000, 5_000_000)]
        public int VectorCount;

        [Params(1, 500, 5000)]
        public int SplitCount;

        private readonly Random deterministicRandom = new Random(Seed: 42);

        [Benchmark(Baseline = true)]
        public void InitIndexWithFatKey()
        {
            DoInitIndex(RandomId, ArrayEqualityComparer<DataPointKeyValueFat>.Instance, "Memory usage after init (baseline): ");
        }

        [Benchmark(Baseline = false)]
        public void InitIndexWithProtobufKey()
        {
            DoInitIndex(CompressedRandomId, ByteArrayComparer.Instance, "Memory usage after init (optimized with protobuf): ");
        }

        [Benchmark(Baseline = false)]
        public void InitIndexWithCompactKey()
        {
            Console.Out.WriteLine($"sizeof(DataPointKeyValueCompact): {Marshal.SizeOf(typeof(DataPointKeyValueCompact))}");

            DoInitIndex(CompactRandomId, ArrayEqualityComparer<DataPointKeyValueCompact>.Instance, "Memory usage after init (optimized with struct layout): ");
        }

        private void DoInitIndex<TId>(Func<TId> newId, IEqualityComparer<TId> idComparer, string message)
            where TId : notnull
        {
            var indexStoreFactory = new IndexStoreFactory<TId, TId>(new SilentLog());

            var indexStores = new IIndexStore<TId, TId, DenseVector>[SplitCount];

            for (var i = 0; i < SplitCount; i++)
            {
                indexStores[i] = indexStoreFactory.Create<DenseVector>(
                    Algorithms.FaissIndexFlatIP,
                    VectorDimension,
                    withDataStorage: false,
                    idComparer);
            }

            var indexDataPoints = ProduceIndexDataPoints(newId);

            foreach (var batch in indexDataPoints.Batch(size: 1000, b => b.ToArray()))
                indexStores[deterministicRandom.Next(SplitCount)].AddBatch(batch);

            LogMemoryUsage(message);

            Console.Out.WriteLine($"Indexes: {indexStores.Length}, Vectors: {indexStores.Sum(x => x.Count)}");

            for (var i = 0; i < SplitCount; i++)
            {
                indexStores[i].Dispose();
                indexStores[i] = null!;
            }

            CollectGarbageWithLohCompaction();
        }

        private IEnumerable<IndexDataPoint<TId, TId, DenseVector>> ProduceIndexDataPoints<TId>(Func<TId> newId)
            where TId : notnull
        {
            for (var i = 0; i < VectorCount; i++)
            {
                yield return new IndexDataPoint<TId, TId, DenseVector>
                {
                    Id = newId(),
                    Vector = RandomVector(),
                    Data = default,
                    IsDeleted = false
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DataPointKeyValueFat[] RandomId()
        {
            return new[]
            {
                DataPointKeyValueFat.Create(Guid.NewGuid()),
                DataPointKeyValueFat.Create(RandomString())
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DataPointKeyValueCompact[] CompactRandomId()
        {
            return new[]
            {
                new DataPointKeyValueCompact(Guid.NewGuid()),
                new DataPointKeyValueCompact(RandomString())
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] CompressedRandomId()
        {
            return AttributeValueSerializer.Serialize(
                new[]
                {
                    new AttributeValue {Guid = Guid.NewGuid()},
                    new AttributeValue {String = RandomString()}
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string RandomString()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 15);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DenseVector RandomVector()
        {
            var coordinates = new double[VectorDimension];
            for (var i = 0; i < VectorDimension; i++)
                coordinates[i] = deterministicRandom.NextDouble();

            return new DenseVector(coordinates);
        }

        private static void LogMemoryUsage(string message)
        {
            CollectGarbageWithLohCompaction();

            var currentProcess = Process.GetCurrentProcess();
            var privateMb = currentProcess.PrivateMemorySize64 / (1024 * 1024);
            var workingSetMb = currentProcess.WorkingSet64 / (1024 * 1024);
            Console.Out.WriteLine($"{message} privateMb: {privateMb}, workingSetMb: {workingSetMb}");
        }

        private static void CollectGarbageWithLohCompaction()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true, compacting: true);
        }

        public class DataPointKeyValueFat
        {
            public string? StringValue { get; set; }
            public Guid? GuidValue { get; set; }
            public bool? BooleanValue { get; set; }

            public override string ToString()
            {
                return StringValue
                       ?? GuidValue?.ToString()
                       ?? BooleanValue?.ToString()
                       ?? string.Empty;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = StringValue != null ? StringValue.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ GuidValue.GetHashCode();
                    hashCode = (hashCode * 397) ^ BooleanValue.GetHashCode();

                    return hashCode;
                }
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;

                return Equals((DataPointKeyValueFat)obj);
            }

            public static DataPointKeyValueFat Create(bool value) => new DataPointKeyValueFat {BooleanValue = value};
            public static DataPointKeyValueFat Create(Guid value) => new DataPointKeyValueFat {GuidValue = value};

            public static DataPointKeyValueFat Create(string value)
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value), "Can't create DataPointKeyValue without value");

                return new DataPointKeyValueFat {StringValue = value};
            }

            public static bool operator==(DataPointKeyValueFat left, DataPointKeyValueFat right)
            {
                return Equals(left, right);
            }

            public static bool operator!=(DataPointKeyValueFat left, DataPointKeyValueFat right)
            {
                return !Equals(left, right);
            }

            protected bool Equals(DataPointKeyValueFat other)
            {
                return
                    string.Equals(StringValue, other.StringValue) &&
                    GuidValue.Equals(other.GuidValue) &&
                    BooleanValue == other.BooleanValue;
            }
        }

        public static class AttributeValueSerializer
        {
            static AttributeValueSerializer()
            {
                var meta = RuntimeTypeModel.Default.Add<AttributeValue>(applyDefaultBehaviour: false, CompatibilityLevel.Level300);
                meta.UseConstructor = false;

                meta.Add(1, nameof(AttributeValue.String));
                meta.Add(2, nameof(AttributeValue.Guid));
                meta.Add(3, nameof(AttributeValue.Bool));
                meta.Add(4, nameof(AttributeValue.Int64));
                meta.Add(5, nameof(AttributeValue.DateTime));

                // note (andrew, 28.07.2021): enforce compact Guid encoding in 16 bytes (http://protobuf-net.github.io/protobuf-net/compatibilitylevel.html)
                meta[2].DataFormat = DataFormat.FixedSize;

                Serializer.PrepareSerializer<AttributeValue[]>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static byte[] Serialize(AttributeValue[] values)
            {
                using var ms = new MemoryStream();
                Serializer.Serialize(ms, values);
                return ms.ToArray();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static AttributeValue[] Deserialize(ReadOnlySpan<byte> bytes)
            {
                return Serializer.Deserialize<AttributeValue[]>(bytes);
            }
        }

        private class JobsConfig : ManualConfig
        {
            private readonly RunMode runMode = new RunMode
            {
                RunStrategy = RunStrategy.ColdStart,
                LaunchCount = 1,
                WarmupCount = 0,
                IterationCount = 1,
                InvocationCount = 1, //ops per iteration
                UnrollFactor = 1
            };

            public JobsConfig()
            {
                SummaryStyle = new SummaryStyle(CultureInfo.InvariantCulture, printUnitsInHeader: false, SizeUnit.MB, TimeUnit.Second);

                AddJob(Job.Dry.UnfreezeCopy().Apply(runMode));
            }
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public unsafe struct DataPointKeyValueCompact : IEquatable<DataPointKeyValueCompact>
        {
            [FieldOffset(0)]
            private readonly byte TypeDescriptor;

            [FieldOffset(1)]
            private readonly bool Bool;

            [FieldOffset(1)]
            private readonly short Int16;

            [FieldOffset(1)]
            private readonly int Int32;

            [FieldOffset(1)]
            private readonly long Int64;

            [FieldOffset(1)]
            private readonly DateTime DateTime;

            [FieldOffset(1)]
            private readonly Guid Guid;

            [FieldOffset(1)]
            private fixed byte ByteArray16[16];

            public DataPointKeyValueCompact(Guid value)
            {
                Bool = default;
                Int16 = default;
                Int32 = default;
                Int64 = default;
                DateTime = default;

                TypeDescriptor = 1;
                Guid = value;
            }

            public DataPointKeyValueCompact(string value)
            {
                Bool = default;
                Int16 = default;
                Int32 = default;
                Int64 = default;
                DateTime = default;
                Guid = default;

                TypeDescriptor = 2;

                var utf8Bytes = Encoding.UTF8.GetBytes(value);
                if (utf8Bytes.Length > 15)
                    throw new InvalidOperationException("value is too long");

                var offset = 0;
                ByteArray16[offset++] = (byte)utf8Bytes.Length;
                foreach (var b in utf8Bytes)
                    ByteArray16[offset++] = b;
            }

            public bool Equals(DataPointKeyValueCompact other) =>
                TypeDescriptor == other.TypeDescriptor && Guid.Equals(other.Guid);

            public override bool Equals(object? obj) =>
                obj is DataPointKeyValueCompact other && Equals(other);

            public override int GetHashCode() =>
                HashCode.Combine(TypeDescriptor, Guid);

            public static bool operator==(DataPointKeyValueCompact left, DataPointKeyValueCompact right) =>
                left.Equals(right);

            public static bool operator!=(DataPointKeyValueCompact left, DataPointKeyValueCompact right) =>
                !left.Equals(right);
        }

        public record AttributeValue(
            string? String = null,
            Guid? Guid = null,
            bool? Bool = null,
            long? Int64 = null,
            DateTime? DateTime = null);
    }
}
