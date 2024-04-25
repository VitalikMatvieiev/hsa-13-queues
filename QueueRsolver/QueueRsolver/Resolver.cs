using System.Diagnostics;
using Droog.Beanstalk.Client;
using StackExchange.Redis;
using Turbocharged.Beanstalk;

namespace QueueRsolver;

public static class Resolver
{
    public static async Task RedisTest(string connectionString, int operations)
    {
        const string channel = "commands";

        ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        
        IDatabase db = redis.GetDatabase();
        
        var producerStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < operations; i++)
        {
            await db.ListRightPushAsync(channel, i.ToString());
        }
        
        producerStopwatch.Stop();
        
        var consumerStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < operations; i++)
        {
            await db.ListLeftPopAsync(channel);
        }
        
        consumerStopwatch.Stop();
        Console.WriteLine(producerStopwatch.ElapsedMilliseconds);
        Console.WriteLine(consumerStopwatch.ElapsedMilliseconds);
    }
    public static async Task BeanstalkTest(string connectionString, int operations)
    {
        var tube = "commands";
        // Create a producer
        IProducer producer = await BeanstalkConnection.ConnectProducerAsync(connectionString);
        await producer.UseAsync(tube);

        // Create a consumer
        IConsumer consumer = await BeanstalkConnection.ConnectConsumerAsync(connectionString);
        await consumer.WatchAsync(tube);

        var producerStopwatch = Stopwatch.StartNew();

        // Produce
        for (int i = 0; i < operations; i++)
        {
            byte[] job = Guid.NewGuid().ToByteArray();

            await producer.PutAsync(job, priority: 0, delay: TimeSpan.Zero, timeToRun: TimeSpan.FromSeconds(1));
        }

        producerStopwatch.Stop();

        var consumerStopwatch = Stopwatch.StartNew();

        // Consume
        for (int i = 0; i < operations; i++)
        {
            var job = await consumer.ReserveAsync(TimeSpan.FromSeconds(5));

            await consumer.DeleteAsync(job.Id);
        }
        
        consumerStopwatch.Stop();
        Console.WriteLine(producerStopwatch.ElapsedMilliseconds);
        Console.WriteLine(consumerStopwatch.ElapsedMilliseconds);
    }
    
    /*void BeanstalkTest((string host,int port) info, int operation)
    {
        using(var client = new BeanstalkClient(info.host, info.port)) {

            for (int i = 0; i < operation; i++)
            {
                var put = client.Put(0, TimeSpan.Zero, TimeSpan.FromSeconds(1), i.ToString());
            }


            // reserve data from queue
            var reserve = client.Reserve();

            // delete reserved data from queue
            client.Delete(reserve.JobId);
        }
    }*/
}