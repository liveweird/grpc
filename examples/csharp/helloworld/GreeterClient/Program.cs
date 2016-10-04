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
using Grpc.Core;
using Helloworld;

namespace GreeterClient
{
    class Program
    {
        public static void Main2(string[] args)
        {
            var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new Greeter.GreeterClient(channel);

            while (!Console.KeyAvailable)
            {
                try
                {
                    var reply = client.SayHello(new HelloRequest
                                                {
                                                    Name = "you"
                                                });
                    Console.WriteLine("Greeting: " + reply.Message);
                }
                catch (RpcException kaboom)
                {
                    Console.WriteLine(kaboom.ToString());
                }

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            channel.ShutdownAsync().Wait();
        }

        public static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel);

            var reply = client.SayHello(new HelloRequest { Name = "you" });
            Console.WriteLine("Greeting: " + reply.Message);

            var replyAsync = client.SayHelloAsync(new HelloRequest
                                                        {
                                                            Name = "you"
                                                        });

            Console.WriteLine("Greeting (async): " + replyAsync.ResponseAsync.Result.Message);

            using (var call = client.SayHelloServerStream(new HelloRequest
                                                          {
                                                              Name = "you"
                                                          }))
            {
                while (call.ResponseStream.MoveNext().Result)
                {
                    Console.WriteLine("Greeting (streamed on server): " + call.ResponseStream.Current);
                }
            }

            using (var call = client.SayHelloClientStream())
            {
                Enumerable.Range(0,
                                 10)
                          .ToList()
                          .ForEach(p =>
                                   {
                                       call.RequestStream.WriteAsync(new HelloRequest
                                       {
                                           Name = "you" + p
                                       }).Wait();
                                   });

                call.RequestStream.CompleteAsync().Wait();
                var response = call.ResponseAsync.Result;

                Console.WriteLine("Greeting (streamed on a client): " + response);
            }


            using (var call = client.SayHelloBiDirectionalStream())
            {
                Enumerable.Range(0,
                                 10)
                          .ToList()
                          .ForEach(p =>
                          {
                              call.RequestStream.WriteAsync(new HelloRequest
                              {
                                  Name = "you" + p
                              }).Wait();
                          });

                call.RequestStream.CompleteAsync().Wait();

                while (call.ResponseStream.MoveNext().Result)
                {
                    Console.WriteLine("Greeting (streamed bi-directionally): " + call.ResponseStream.Current);
                }
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
