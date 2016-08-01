//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.VisualStudio.ComponentModelHost;
using System;
using System.Collections.Generic;

namespace DockerTools2
{
    internal static class ServiceProviderExtensions
    {
        public static TInterfaceType GetService<TInterfaceType, TServiceType>(this IServiceProvider serviceProvider)
            where TInterfaceType : class
            where TServiceType : class
        {
            return serviceProvider.GetService(typeof(TServiceType)) as TInterfaceType;
        }

        public static TInterfaceType GetRequiredService<TInterfaceType, TServiceType>(this IServiceProvider serviceProvider)
            where TInterfaceType : class
            where TServiceType : class
        {
            var service = serviceProvider.GetService<TInterfaceType, TServiceType>();

            if (service == null)
            {
                throw new InvalidOperationException("The required service is not found.");
            }

            return service;
        }

        public static TInterfaceType GetService<TInterfaceType>(this IServiceProvider serviceProvider)
            where TInterfaceType : class
        {
            return serviceProvider.GetService<TInterfaceType, TInterfaceType>();
        }

        public static TInterfaceType GetRequiredService<TInterfaceType>(this IServiceProvider serviceProvider)
            where TInterfaceType : class
        {
            return serviceProvider.GetRequiredService<TInterfaceType, TInterfaceType>();
        }

        public static TInterfaceType GetServiceFromComponentModel<TInterfaceType>(this IServiceProvider serviceProvider)
            where TInterfaceType : class
        {
            var componentModel = serviceProvider.GetRequiredService<IComponentModel, SComponentModel>();

            return componentModel.GetService<TInterfaceType>();
        }

        public static IEnumerable<TInterfaceType> GetExtensionsFromComponentModel<TInterfaceType>(this IServiceProvider serviceProvider)
            where TInterfaceType : class
        {
            var componentModel = serviceProvider.GetRequiredService<IComponentModel, SComponentModel>();

            return componentModel.GetExtensions<TInterfaceType>();
        }
    }
}
