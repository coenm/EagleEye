﻿// ReSharper disable once CheckNamespace
namespace EagleEye.Categories
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Xunit.Sdk;

    [TraitDiscoverer(ExifToolDiscoverer.DISCOVERER_TYPE_NAME, TestHelperSettings.ASSEMBLY_NAME)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Xunit")]
    public class ExifToolAttribute : Attribute, ITraitAttribute
    {
    }
}