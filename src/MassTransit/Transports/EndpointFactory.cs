// Copyright 2007-2011 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using Exceptions;
    using Internal;

    public class EndpointFactory :
        IEndpointFactory
    {
        readonly IList<ITransportFactory> _factories;

        public EndpointFactory(IList<ITransportFactory> factories)
        {
            _factories = factories;
        }


        public IEndpoint BuildEndpoint(Uri uri, Action<IEndpointConfigurator> configurator)
        {
            foreach (var factory in _factories)
            {
                try
                {
                    if (uri.Scheme.ToLowerInvariant() == factory.Scheme)
                    {
                    	var endpointConfigurator = new EndpointConfigurator(uri);
                        
						EndpointSettings endpointSettings = endpointConfigurator.New(configurator);

                        var transport = factory.BuildLoopback(endpointSettings.Normal);
                        var errorTransport = factory.BuildError(endpointSettings.Error);

                        var endpoint = new Endpoint(transport.Address, endpointSettings.Normal.Serializer, transport, errorTransport);

                        return endpoint;
                    }
                }
                catch (Exception ex)
                {
                    throw new EndpointException(uri, "Error", ex);
                }
            }

            throw new ConfigurationException("No transport could handle: '{0}'".FormatWith(uri));
        }

        public void AddTransportFactory(ITransportFactory factory)
        {
            _factories.Add(factory);
        }
    }
}