﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Primitives;

namespace Plato.Internal.Layout.Theming
{
    public class ThemingViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = ViewPath.NormalizePath("/_ViewStart" + RazorViewEngine.ViewExtension),
                ViewAttribute = new RazorViewAttribute("/_ViewStart" + RazorViewEngine.ViewExtension, typeof(ThemeViewStart)),
                IsPrecompiled = true
            });

        }
    }
}
