﻿using Be.IO;
using System;
using System.IO;
using System.Text;
using ValveKeyValue.Abstraction;

namespace ValveKeyValue.Serialization
{
    sealed class KV2BinarySerializer : IVisitationListener, IDisposable
    {
        public KV2BinarySerializer(Stream stream)
        {
            Require.NotNull(stream, nameof(stream));

            writer = new BeBinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        }

        readonly BeBinaryWriter writer;
        int objectDepth;

        public void Dispose()
        {
            writer.Dispose();
        }

        public void OnObjectStart(string name)
        {
            objectDepth++;
            Write(KV2BinaryNodeType.ChildObject);
            Write(name);
        }

        public void OnObjectEnd()
        {
            Write(KV2BinaryNodeType.End);

            objectDepth--;
            if (objectDepth == 0)
            {
                Write(KV2BinaryNodeType.End);
            }
        }

        public void OnKeyValuePair(string name, KVValue value)
        {
            Write(GetNodeType(value.ValueType));
            Write(name);

            switch (value.ValueType)
            {
                case KVValueType.FloatingPoint:
                    writer.Write((float)value);
                    break;

                case KVValueType.Int32:
                case KVValueType.Pointer:
                    writer.Write((int)value);
                    break;

                case KVValueType.String:
                    Write((string)value);
                    break;

                case KVValueType.UInt64:
                    writer.Write((ulong)value);
                    break;
                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(value.ValueType), value.ValueType, "Value was of an unsupported type.");
            }
        }

        void Write(KV2BinaryNodeType nodeType)
        {
            writer.Write((byte)nodeType);
        }

        void Write(string value)
        {
            foreach (var @char in value)
            {
                writer.Write(@char);
            }

            writer.Write((byte)0);
        }

        static KV2BinaryNodeType GetNodeType(KVValueType type)
        {
            switch (type)
            {
                case KVValueType.FloatingPoint:
                    return KV2BinaryNodeType.Float32;

                case KVValueType.Int32:
                    return KV2BinaryNodeType.Int32;

                case KVValueType.Pointer:
                    return KV2BinaryNodeType.Pointer;

                case KVValueType.String:
                    return KV2BinaryNodeType.String;

                case KVValueType.UInt64:
                    return KV2BinaryNodeType.UInt64;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported value type.");
            }
        }
    }
}