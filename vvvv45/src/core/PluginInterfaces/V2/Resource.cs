﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    public enum DestroyReason
    {
        DeviceLost,
        Dispose
    }
    
    [ComVisible(false)]
    public abstract class Resource<TDevice, TResource, TMetadata> : IDisposable 
        where TResource : IDisposable
    {
        private readonly Func<TMetadata, TDevice, TResource> FCreateResourceFunc;
        private readonly Action<TMetadata, TResource> FUpdateResourceFunc;
        private readonly Action<TMetadata, TResource, DestroyReason> FDestroyResourceAction;
        private readonly Dictionary<TDevice, TResource> FResources = new Dictionary<TDevice, TResource>();
        
        public Resource(
            TMetadata metadata, 
            Func<TMetadata, TDevice, TResource> createResourceFunc, 
            Action<TMetadata, TResource> updateResourceFunc = null, 
            Action<TMetadata, TResource, DestroyReason> destroyResourceAction = null)
        {
            Metadata = metadata;
            FCreateResourceFunc = createResourceFunc;
            FUpdateResourceFunc = updateResourceFunc;
            FDestroyResourceAction = destroyResourceAction ?? DestroyResource;
            NeedsUpdate = true;
        }

        /// <summary>
        /// Some arbitrary data associated with this resource.
        /// </summary>
        public TMetadata Metadata { get; set; }

        /// <summary>
        /// Whether or not the Update method has to be called for this resource.
        /// By default this flag is true.
        /// Note: The Update method will always get called for new resources.
        /// </summary>
        public bool NeedsUpdate { get; set; }
        public TResource this[TDevice device]
        {
            get
            {
                TResource result;
                if (!FResources.TryGetValue(device, out result))
                {
                    result = CreateResoure(Metadata, device);
                    NeedsUpdate = true;
                    FResources[device] = result;
                }
                return result;
            }
        }

        public IEnumerable<TResource> DeviceResources { get { return FResources.Values; } }

        protected virtual TResource CreateResoure(TMetadata metadata, TDevice device)
        {
            return FCreateResourceFunc(metadata, device);
        }
        
        public void UpdateResource(TDevice device)
        {
            TResource resource;
            if (FResources.TryGetValue(device, out resource))
            {
                if (NeedsUpdate)
                {
                    FUpdateResourceFunc?.Invoke(Metadata, resource);
                }
            }
            else
            {
                FUpdateResourceFunc?.Invoke(Metadata, this[device]);
            }
        }
        
        public void DestroyResource(TDevice device)
        {
            TResource resource;
            if (FResources.TryGetValue(device, out resource))
            {
                FResources.Remove(device);
                FDestroyResourceAction(Metadata, resource, DestroyReason.DeviceLost);
            }
        }
        
        public void Dispose()
        {
            foreach (var resource in FResources.Values)
            {
                FDestroyResourceAction(Metadata, resource, DestroyReason.Dispose);
            }
            
            FResources.Clear();
        }
        
        private static void DestroyResource(TMetadata metadata, TResource resource, DestroyReason reason)
        {
            if (resource != null)
            {
                resource.Dispose();
            }
        }
    }
}
