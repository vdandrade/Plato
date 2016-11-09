﻿using System.Collections.Generic;
using System.Data;

namespace Plato.Abstractions.Collections
{
    public interface IPagedResults<T> where T : class
    {
        int Total { get; set; }

        IList<T> Data { get; set; }

        void PopulateTotal(IDataReader reader);
    }
}