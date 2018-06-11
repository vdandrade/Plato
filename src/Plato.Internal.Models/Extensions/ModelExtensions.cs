﻿using Newtonsoft.Json;

namespace Plato.Internal.Models.Extensions
{
    public static class ModelExtensions 
    {

        public static string Serialize<T>(this IModel<object> model)
        {
            return JsonConvert.SerializeObject(model);
        }


    }
}