## Xels DNS Crawler 
The Xels DNS Crawler provides a list of Xels full nodes that have recently been active via a custom DNS server.

### Prerequisites

To install and run the DNS Server, you need
* [.NET Core 2.0](https://www.microsoft.com/net/download/core)
* [Git](https://git-scm.com/)

## Build instructions

### Get the repository and its dependencies

```
git clone https://github.com/Xelsproject/XelsBitcoinFullNode.git  
cd XelsBitcoinFullNode
git submodule update --init --recursive
```

### Build and run the code
With this node, you can run the DNS Server in isolation or as a Xels node with DNS functionality:

1. To run a <b>Xels</b> node <b>only</b> on <b>MainNet</b>, do
```
cd Xels.XelsDnsD
dotnet run -dnslistenport=5399 -dnshostname=dns.Xelsplatform.com -dnsnameserver=ns1.dns.Xelsplatform.com -dnsmailbox=admin@Xelsplatform.com
```  

2. To run a <b>Xels</b> node and <b>full node</b> on <b>MainNet</b>, do
```
cd Xels.XelsDnsD
dotnet run -dnsfullnode -dnslistenport=5399 -dnshostname=dns.Xelsplatform.com -dnsnameserver=ns1.dns.Xelsplatform.com -dnsmailbox=admin@Xelsplatform.com
```  

3. To run a <b>Xels</b> node <b>only</b> on <b>TestNet</b>, do
```
cd Xels.XelsDnsD
dotnet run -testnet -dnslistenport=5399 -dnshostname=dns.Xelsplatform.com -dnsnameserver=ns1.dns.Xelsplatform.com -dnsmailbox=admin@Xelsplatform.com
```  

4. To run a <b>Xels</b> node and <b>full node</b> on <b>TestNet</b>, do
```
cd Xels.XelsDnsD
dotnet run -testnet -dnsfullnode -dnslistenport=5399 -dnshostname=dns.Xelsplatform.com -dnsnameserver=ns1.dns.Xelsplatform.com -dnsmailbox=admin@Xelsplatform.com
```  

### Command-line arguments

| Argument      | Description                                                                          |
| ------------- | ------------------------------------------------------------------------------------ |
| dnslistenport | The port the Xels DNS Server will listen on                                       |
| dnshostname   | The host name for Xels DNS Server                                                 |
| dnsnameserver | The nameserver host name used as the authoritative domain for the Xels DNS Server |
| dnsmailbox    | The e-mail address used as the administrative point of contact for the domain        |

### NS Record

Given the following settings for the Xels DNS Server:

| Argument      | Value                             |
| ------------- | --------------------------------- |
| dnslistenport | 53                                |
| dnshostname   | Xelsdns.Xelsplatform.com    |
| dnsnameserver | ns.Xelsdns.Xelsplatform.com |

You should have NS and A record in your ISP DNS records for your DNS host domain:

| Type     | Hostname                          | Data                              |
| -------- | --------------------------------- | --------------------------------- |
| NS       | Xelsdns.Xelsplatform.com    | ns.Xelsdns.Xelsplatform.com |
| A        | ns.Xelsdns.Xelsplatform.com | 192.168.1.2                       |

To verify the Xels DNS Server is running with these settings run:

```
dig +qr -p 53 Xelsdns.Xelsplatform.com
```  
or
```
nslookup Xelsdns.Xelsplatform.com
```
