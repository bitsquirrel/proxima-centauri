using System;
using System.Reflection;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;

namespace Functions_for_Dynamics_Operations
{
    /// <summary>
    /// Installs <see cref="CustomLabelService"/> into Microsoft''s static <c>AxServiceProvider</c>
    /// so any caller of <c>AxServiceProvider.GetService&lt;ILabelService&gt;()</c> receives our
    /// wrapper. Decompiled findings (10.0.48):
    ///  - <c>AxServiceProvider.serviceProvider</c> is a private static field, set first-write-wins via <c>SetSite</c>.
    ///  - <c>CoreUtility.ServiceProvider</c> is just a passthrough to <c>AxServiceProvider.ServiceProvider</c>,
    ///    so patching <c>AxServiceProvider</c> covers the property-grid "..." label button too.
    ///  - Microsoft''s <c>LabelService</c> may be registered after us, so the chained provider
    ///    resolves the underlying <c>ILabelService</c> lazily on every request.
    /// </summary>
    internal static class CustomLabelServiceRegistrar
    {
        private static bool _installed;

        public static bool Install(AsyncPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (_installed) return true;

            try
            {
                FieldInfo field = typeof(AxServiceProvider).GetField(
                    "serviceProvider",
                    BindingFlags.Static | BindingFlags.NonPublic);

                if (field == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "CustomLabelServiceRegistrar: AxServiceProvider.serviceProvider field not found; cannot install.");
                    return false;
                }

                IServiceProvider innerProvider = field.GetValue(null) as IServiceProvider;
                if (innerProvider == null)
                {
                    // Microsoft has not called SetSite yet; caller should retry on a later event.
                    return false;
                }

                if (innerProvider is LazyLabelInterceptingProvider)
                {
                    _installed = true;
                    return true;
                }

                IServiceProvider chained = new LazyLabelInterceptingProvider(innerProvider, package);
                field.SetValue(null, chained);
                _installed = true;

                System.Diagnostics.Debug.WriteLine(
                    "CustomLabelServiceRegistrar: AxServiceProvider patched; default label entry points now route to the custom editor.");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "CustomLabelServiceRegistrar.Install failed: " + ex);
                return false;
            }
        }

        /// <summary>
        /// Resolves the real <see cref="ILabelService"/> lazily on every request and returns a
        /// shared <see cref="CustomLabelService"/> wrapper around it. Independent of install timing.
        /// </summary>
        private sealed class LazyLabelInterceptingProvider : IServiceProvider
        {
            private readonly IServiceProvider _inner;
            private readonly AsyncPackage _package;
            private CustomLabelService _wrapper;
            private ILabelService _wrappedOriginal;
            private readonly object _gate = new object();

            public LazyLabelInterceptingProvider(IServiceProvider inner, AsyncPackage package)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _package = package ?? throw new ArgumentNullException(nameof(package));
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ILabelService))
                {
                    ILabelService current = _inner.GetService(serviceType) as ILabelService;
                    if (current == null) return null;

                    lock (_gate)
                    {
                        if (!ReferenceEquals(current, _wrappedOriginal) || _wrapper == null)
                        {
                            _wrappedOriginal = current;
                            _wrapper = new CustomLabelService(current, _package);
                        }
                        return _wrapper;
                    }
                }

                return _inner.GetService(serviceType);
            }
        }
    }
}
