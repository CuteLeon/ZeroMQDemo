# ZeroMQ - 轻量级消息通讯组件

## 特点：

### **点对点无中间节点**

​	ZeroMQ默认不使用消息服务器，而是将直接点对点通信，消息缓存在发送端。

### **强调消息收发模式**

#### **以统一接口支持多种底层通信方式**

​	不管是线程间通信，进程间通信还是跨主机通信，ZeroMQ都使用同一套API进行调用，只需要更改连接地址的通信协议名称。

- TCP: 在主机之间进行通讯
- INROC: 在同一进程的线程之间进行通讯（线程间）
- IPC : 同一主机的进程之间进行通讯
- PGM: 多播通讯

#### **异步，强调性能**

​	ZeroMQ设计之初就是为了高性能的消息发送而服务的，所以其设计追求简洁高效。它发送消息是异步模式，通过单独出一个IO线程来实现。



## 相关库：

### NetMQ

> 原生C#，是对 ZeroMQ 的 C# 封装，更易用；
>
> \> .NetStandard 1.6\2.0
>
> \> .NetFramework 4.0
>
> 社区活跃
>
> https://github.com/zeromq/netmq
>
> Nuget: NetMQ 4.0.0.207(2019年7月1日)



### clrzmq4

> C#  (.NET 4 and mono 3)
>
> libzmq v4.1.0.x
>
> 社区较为活跃
>
> https://github.com/zeromq/clrzmq4
>
> Nuget: ZeroMQ 4.1.0.31(2019年3月13日)



### Castle.Zmq

> LGPL协议
>
> 现代MQ框架，可以使用常见的ZMQ模式
>
> 社区不活跃
>
> https://github.com/castleproject-deprecated/castlezmq



## clrzmq4：

### 常用模式：

#### C/S模式：

> 客户端/服务端模式；

​	这个经典的模式在 zeroMQ 中是应答状态的，不能同时 send 多个数据，客户端和服务端必须一呼一应。



#### Pub/Sub模式：

> 发布/订阅模式；

​	这里的发布与订阅角色是绝对的，即发布者无法接收消息，订阅者不能发送消息，并且订阅者需要订阅主题或者主题的前缀。

​	当存在多个订阅者订阅同一发布者的相同主题时，同一个消息将被多个订阅者同时接收到。

​	当发布者尚无订阅者连接时，消息将被消耗，不会被维护和重新发送。

> 两个可能的问题：
>
> 1. 可能发布者刚启动时发布的数据出现丢失，原因是发送速度太快，在建立联系时，已经开始了数据发布；
> 2. 当订阅者消费慢于发布时会在发布者处出现数据的堆积，显然，这是不可以被接受的。



#### Push/Pull模式：

> 推拉模式

​	上游发送消息给工人，工人处理后继续下发给下游；

> 可能的问题：
>
> 1. 同 Pub/Sub 模式，通信依然是单向的。
> 2. 当不存在消费者时，发布者的消息不会被消耗，发布者将阻塞以维护消息。
> 3. 多个消费者连接发布者时，发布者的每个消息将发布给唯一的消费者，所有消费者不会收到唯一消息。



### 三种模式适用场景：

- **C/S模式**：远程调用、任务分配、强调一呼一应。
- **Pub/Sub模式**：数据分发。
- **Push/Pull模式**：多任务并行，使用多个消费者负载均衡。



### Socket有效角色绑定：

- PUB <=> SUB
- REQ <=> REP
- REQ <=> XREP
- XREQ <=> REP
- XREQ <=> XREP
- XREQ <=> XREQ
- XREP <=> XREP
- PUSH <=> PULL
- PAIR <=> PAIR



## 例子：

https://github.com/booksbyus/zguide/tree/master/examples/C%23