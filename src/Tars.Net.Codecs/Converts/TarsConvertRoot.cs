﻿using DotNetty.Buffers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tars.Net.Codecs
{
    public class TarsConvertRoot : ITarsConvertRoot
    {
        private readonly ConcurrentDictionary<(Codec, Type, short), ITarsConvert> dict = new ConcurrentDictionary<(Codec, Type, short), ITarsConvert>();
        private readonly ITarsConvert[] converts;

        public int Order => 0;

        public Codec Codec => Codec.Tars;

        public TarsConvertRoot(IEnumerable<ITarsConvert> converts)
        {
            this.converts = converts.OrderBy(i => i.Order).ToArray();
        }

        private ITarsConvert GetConvert(Codec codec, Type type, TarsConvertOptions options)
        {
            return dict.GetOrAdd((codec, type, options.Version), (op) =>
            {
                var convert = converts.FirstOrDefault(i => i.Accept(op));
                if (convert == null)
                {
                    throw new NotSupportedException($"Codecs not supported {options}.");
                }
                return convert;
            });
        }

        public void Serialize(object obj, IByteBuffer buffer, int order, bool isRequire = true, TarsConvertOptions options = null, Codec codec = Codec.Tars)
        {
            var op = options ?? TarsConvertOptions.Default;
            GetConvert(codec, obj.GetType(), op).Serialize(obj, buffer, order, isRequire, op);
        }

        public bool Accept((Type, short) options)
        {
            return true;
        }

        public object Deserialize(IByteBuffer buffer, Type type, int order, bool isRequire = true, TarsConvertOptions options = null, Codec codec = Codec.Tars)
        {
            var op = options ?? TarsConvertOptions.Default;
            return GetConvert(codec, type, op).Deserialize(buffer, type, order, isRequire, op);
        }

        public bool Accept((Codec, Type, short) options)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IByteBuffer buffer, Type type, int order, bool isRequire = true, TarsConvertOptions options = null)
        {
            throw new NotImplementedException();
        }

        public void Serialize(object obj, IByteBuffer buffer, int order, bool isRequire = true, TarsConvertOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}