// See https://aka.ms/new-console-template for more information


using System.Diagnostics;
using 高并发下生成单据号.Provider;

public class Program
{
    public static readonly int ThreadCount =10; //并发个数
    //正常一般数据库30个并发，不会有特别的情况出现，如果你要测试的话，你可以把对应的方法加上Thread.Sleep(2000)
    

    static Program()
    {
        //进行连接预热，虽然这样启动会耗时一点。
        //这个不一定需要这么在这里做，这里只是为了演示。

        //这个连接池里面的连接数量，可以通过数据库连接字符串的连接参数进行设置。

        Console.WriteLine("连接池预热开始");
        for (var i = 0; i < ThreadCount; i++)
        {
            BarcodeProvider.GetDbNow();
        }
        Console.WriteLine("连接池预热结束");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("并发场景下，生成单据号");



        Stopwatch sw = Stopwatch.StartNew();

        sw.Start();
        var generateBillNoTasks = new List<Task>();


        #region 普通的开启ThreadCount数量线程进行调用

        //Console.WriteLine("普通方式调用");
        //for (int i = 0; i < ThreadCount; i++)
        //{
        //    generateBillNoTasks.Add(Task.Factory.StartNew(() =>
        //     {
        //         Console.WriteLine(BarcodeProvider.GenerateBillNo("TEST01", "2021", "*", "lisi"));
        //     }));
        //}


        #endregion


        #region 数据库层面的悲观锁
        //Console.WriteLine("悲观锁方式");
        //for (int i = 0; i < ThreadCount; i++)
        //{
        //    generateBillNoTasks.Add(Task.Factory.StartNew(() =>
        //    {
        //        Console.WriteLine(BarcodeProvider.GenerateBillNoByPessimisticLock("TEST09", "2021", "*", "lisi"));
        //    }));
        //}
        #endregion


        #region 数据库层面的乐观锁
        Console.WriteLine("乐观锁方式");
        for (int i = 0; i < ThreadCount; i++)
        {
            generateBillNoTasks.Add(Task.Factory.StartNew(() =>
             {
                 Console.WriteLine(
                     BarcodeProvider.GenerateBillNoByOptimisticLock("TEST110", "2021", "*", "lisi")
                     );
             }));
        }

        #endregion

        Task.WaitAll(generateBillNoTasks.ToArray());
        sw.Stop();

        Console.WriteLine($"并发数:{ThreadCount}时生成单据号耗时(毫秒):{sw.ElapsedMilliseconds}");
        Console.ReadKey();


    }
}



















//先说高并发下结论：
//有乐观锁(不重试机制）和悲观锁


//悲观锁
//优点：
//1、实现简单，
//2、百分百能保证成功
//缺点：
//1、悲观锁的效率不高，扛不住高并发的场景，不过一般的场景也够用了。


//乐观锁(不重试)
//优点：能够高并发，其实代码也相对简单
//缺点：可能会有失败的情况


//乐观锁(重试机制）
//优点：结合乐观锁和悲观锁的优缺点
//缺点：实现相对复杂


//要支持高并发，要进行数据库连接的预热


//方法一、数据库层面的悲观锁
//实现思路：利用数据库的事务机制

//如果在一个事务中修改某一行的时候，而另外一个事务也同时修改某一行的时候，必须等待其他事务修改提交，另外一个修改才能继续执行下去。当更新语句执行成功的时候，再从这个事务进行select（本次修改，这个事务可见） ，最后提交事务。
//这种方式，简单容易实现，并且不容易造成bug，虽然当有大量并发进来的时候，大家都在等最先进去的事务进行提交。

//也有说用for update 但是我不太推荐，1、不是所有数据库都有for update。2、如果for update 太多可能会造成死锁【如果你通篇都是for update 就可能会导致死锁】


//方法二、乐观锁
//实现思路:类似于版本号机制,因为单据号表里面的刚刚好有个序号的字段，可以用来做，就不用搞单独版本号字段了。


//高并发场景下，需要对数据库连接进行预热，Oracle.ManagedDataAccess.Core内部帮我们实现了数据库连接池，我们只需要进行连接池预热即可。
//如果不预热的话，当高并发数据进来的时候，会导致大家都新建立数据库连接去查询【而建连接是比较耗时的操作】，会影响我们的系统对接受并发的个数














