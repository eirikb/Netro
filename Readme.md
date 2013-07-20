## Netro


Reverse tunneling in Windows.  
I usually fire up ´ssh -R´ when I need some reverse tunneling, on any platform.
But sometimes I have to tunnel from Windows to Windows, and this is a bit more difficult, as I usually don't have sshd running on Windows servers.  
Netro, a bad play on "Metro" is here to help me and others that must do some quick, dirty and simple tunneling on Windows.


Here is an example:
The right side in the following figure is behind a firewall, the left side is publicly accessible.

1.  Netro A listen for incoming connections, and listens for reverse tunneling.
2.  Netro B connect to A for tunneling.
3.  When a client connect to A the data will be transferred through the link between A and B.
4.  Connection is made on B to desired host:port.
5.  Data can now flow both ways.


                        ┊
        u               ┆               O
        │               ┆               │ 
    ┌───┴───┐           ┆           ┌───┴───┐
    │       │           ┆           │       │
    │   A   ├───ᑕO──────┆───────────┤   B   │
    │       │           ┆           │       │
    └───────┘           ┆           └───────┘
                        ┆
                        
                        
 
 ### Tunneling types
 
 ´Netro.exe 5000 localhost:80´  
 Listen on port 5000, send connections/data to localhost on port 80.
 No reverse tunneling involved.
 
        ┌─────┐
    ᑐ───┤     ├───O
        └─────┘
        
´Netro.exe 5000 5001´  
Listen on port 5000 for normal connections. Listen on port 5001 for reverse tunneling connection.

           u
           │
        ┌──┴──┐
    ᑐ───┤     │
        └─────┘
        
´Netro.exe example.com:5001 localhost:80´  
Open reverse tunneling cunnection against example.com on port 5001.
On reverse connections, open connection against localhost on port 80.

           u
           │
        ┌──┴──┐
        │     ├───O
        └─────┘
        