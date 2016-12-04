// Copyright 2015, Google Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }

        public override async Task SayHelloServerStream(HelloRequest request,
                                                  IServerStreamWriter<HelloReply> responseStream,
                                                  ServerCallContext context)
        {
            foreach (var p in Enumerable.Range(0,
                                               10))
            {
                await responseStream.WriteAsync(new HelloReply
                                          {
                                              Message =
                                                  "Hello " + request.Name +
                                                  p
                                          });
            }
        }

        public override async Task<HelloReply> SayHelloClientStream(IAsyncStreamReader<HelloRequest> requestStream,
                                                                    ServerCallContext context)
        {
            var builder = new StringBuilder();

            while (await requestStream.MoveNext())
            {
                var current = requestStream.Current;
                builder.AppendLine("Hello " + current.Name);
            }

            return new HelloReply { Message = builder.ToString() };
        }

        public override async Task SayHelloBiDirectionalStream(IAsyncStreamReader<HelloRequest> requestStream,
                                                               IServerStreamWriter<HelloReply> responseStream,
                                                               ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var current = requestStream.Current;

                await responseStream.WriteAsync(new HelloReply
                                                {
                                                    Message =
                                                        "Hello " + current.Name
                                                });
            }
        }
    }

    /*
     * TODO:
     * 1. Add non-breaking change
     * 2. Add breaking change
     * _X_ 3. Check interoperability between various technologies
     * 4. Mismatch detection
     * 5. Performance comparison (serialization / deserialization) - https://performance-dot-grpc-testing.appspot.com/
     * 6. Types allowed in Protobuf
     * 7. Compare load size in JSON (RESTful) & gRPC (Protobuf)
     * _X_ 8. Error handling (broken connection handling) - internal health check
     * _X_ 9. Connection pooling & load balancing
     * 10. Compare with Finagle, Avro, Thrift, anything else? ZeroMQ
     * 11. Who uses & what scale?
     * 12. https://github.com/grpc-ecosystem/polyglot
     * 13. https://github.com/grpc-ecosystem/grpc-gateway
     * 14. Add security
     */

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
