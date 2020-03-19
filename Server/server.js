const WebSocket = require('ws');

class Server {
  constructor(port) {
    this.clients = []
    this.users = []
    this.userUniqueId = 1
    this.ws = new WebSocket.Server({ port: port })
    this.ws.on('connection',this.connectionListener.bind(this))
    p(`server running at port ${port}\n`)
  }

  /** Initializing the client when connecting */
  connectionListener(ws, request) {
    // 掃除処理
    this.clients = this.clients.filter(c => c.readyState === 1)

    // socketに名前をつける
    ws.name = ws._socket.remoteAddress + ":" + `${Math.random()}`.slice(2, 14)
    this.clients.push(ws)
    p(`Join ${ws.name}`)
    this.emit(ws, { Type: 'join' })

    ws.on('message', data => {
      let d = JSON.parse(data)
      switch(d.Type){
      case 'updateUser':
        let msg = JSON.parse(d.Data)
        this.users = this.users.filter(c => c.Id !== msg.User.Id)
        this.users.push(msg.User)
        this.broadcast(ws, data)
        break
      case 'gameStart':
        // 参加
        let c = this.createUser(d.Data, ws.name)
        this.users.push(c)
        this.emit(ws, {
          Type: 'gameStart',
          Data: JSON.stringify({
            Users: this.users,
            Player: c,
          }),
        })
        break
      default:
        this.broadcast(ws, data)
      }
    })

    ws.on('close', () => {
      this.users = this.users.filter(c => c.WsName !== ws.name)
      this.clients.slice(this.clients.indexOf(ws), 1)
      this.broadcast(ws, JSON.stringify({
        Type: 'exitUser',
        Data: JSON.stringify({
          WsName: ws.name,
        })
      }))
      p(`Exit ${ws.name}`)
    })
  }

  createUser(name, WsName){
    return {
      Id: this.userUniqueId++,
      WsName: WsName === undefined? "" : WsName,
      Name: name === undefined? "" : name,
      Hp: 10,
      Power: 0,
      Angle: 0,
      X: -8 + round(Math.random() * 16, 1000),
      Y: -8 + round(Math.random() * 16, 1000),
      IsDash: false
    }
  }

  emit(ws, data){
    ws.send(JSON.stringify(data))
  }

  /** Push to other clients */
  broadcast(sender, message) {
    for (let c of this.clients) {
      if (c.readyState === 1) {
        c.send(message)
      }
    }
  }

  /** close server */
  close() {
    this.server.close()
  }
}

function round(value, base) {
    return Math.round(value * base) / base;
}

function p(message) {
  process.stdout.write(message + '\n')
}

module.exports = Server
