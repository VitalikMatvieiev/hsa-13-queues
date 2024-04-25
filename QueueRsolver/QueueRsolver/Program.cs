using QueueRsolver;

(string, int) redisconectionAOF = ("localhost:6380", 1000000);
(string, int) redisconectionRDB = ("localhost:6381", 1000000);
(string, int) beanstalkdConnection = ("localhost:11300", 1000000);

Resolver.BeanstalkTest(beanstalkdConnection.Item1, beanstalkdConnection.Item2).Wait();
//Resolver.RedisTest(redisconectionRDB.Item1, redisconectionRDB.Item2).Wait();