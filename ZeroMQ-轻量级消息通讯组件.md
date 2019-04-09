# ZeroMQ - 轻量级消息通讯组件

## 特点：

### **点对点无中间节点**

​	ZeroMQ默认不使用消息服务器，而是将直接点对点通信，消息缓存在发送端。

### **强调消息收发模式**

#### **以统一接口支持多种底层通信方式**

​	不管是线程间通信，进程间通信还是跨主机通信，ZeroMQ都使用同一套API进行调用，只需要更改通信协议名称。

- TCP: 在主机之间进行通讯
- INROC: 在同一进程的线程之间进行通讯（线程间）
- IPC : 同一主机的进程之间进行通讯
- PGM: 多播通讯

#### **异步，强调性能**

​	ZeroMQ设计之初就是为了高性能的消息发送而服务的，所以其设计追求简洁高效。它发送消息是异步模式，通过单独出一个IO线程来实现。



## 相关库：

### NetMQ

> 原生C#
>
> \> .NetStandard 1.3
>
> \> .NetFramework 4.0
>
> 社区活跃
>
> https://github.com/zeromq/netmq
>
> Nuget: NetMQ 4.0.0.207-pre(2019年1月24日)



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
> 现在MQ框架，可以使用常见的ZMQ模式
>
> 社区不活跃
>
> https://github.com/castleproject-deprecated/castlezmq



## 例子：

https://github.com/booksbyus/zguide/tree/master/examples/C%23





## clrzmq4：

### C/S模式：

​	这个经典的模式在 zeroMQ 中是应答状态的，不能同时 send 多个数据，客户端和服务端必须一呼一应。