const WebSocket = require('ws');

class Server {
  constructor(port) {
    this.clients = []
    this.users = []
    this.Zombie = []
    this.ZombieId = 1
    this.userUniqueId = 1
    this.ws = new WebSocket.Server({ port: port })
    this.ws.on('connection', this.connectionListener.bind(this))
    p(`server running at port ${port}\n`)
    setInterval((() => {
      if(this.Zombie.length < 100){
        var zombie = this.createZombie()
        this.Zombie.push(zombie)
        this.broadcast(this.ws, JSON.stringify({
          Type: 'createZombie',
          Data: JSON.stringify({
            zombie: this.zombie,
          })
        }))
        p(`Zombie${zombie.Id} join`)
      }
    }).bind(this), 1)
  }     

  createZombie() {
    return {
      Id: this.ZombieId++,
      HP: 100,
    }
  }

  connectionListener(ws, request) {
    this.clients = this.clients.filter(c => c.readyState === 1)

    ws.name = ws._socket.remoteAddress + ":" + `${Math.random()}`.slice(2, 14)
    this.clients.push(ws)
    p(`Join ${ws.name}`)
    this.emit(ws, { Type: 'join' })

    ws.on('message', data => {
      let d = JSON.parse(data)
      switch (d.Type) {
        case 'ZombieLockPlayer':
          let z = JSON.parse(d.Data)
          let zb = z.zombie
          this.Zombie = this.Zombie.filter(c => c.Id !== z.zombie.Id)
          this.Zombie.push(z.zombie)
          this.broadcast(ws, data)
          if(zb.HP === 0) { 
            this.Zombie.splice(this.Zombie.findIndex(function(item, index){if(item.Id == zb.Id)return true}), 1)
          }
          break
        case 'updateUser':
          let msg = JSON.parse(d.Data)
          this.users = this.users.filter(c => c.Id !== msg.User.Id)
          this.users.push(msg.User)
          this.broadcast(ws, data)
          if(msg.User.HP === 0) { 
            this.users.splice(this.users.findIndex(function(item, index){if(item.Id == msg.User.Id)return true}), 1)
          }
          break
        case 'gameStart':
          let c = this.createUser(d.Data, ws.name)
          this.users.push(c)
          this.emit(ws, {
            Type: 'gameStart',
            Data: JSON.stringify({
              Users: this.users,
              zombie: this.Zombie,
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

  createUser(name, WsName) {
    return {
      Id: this.userUniqueId++,
      WsName: WsName === undefined ? "" : WsName,
      Name: name === undefined ? "" : name,
      HP: 100,
      Score: 0,
      Angle: 0,
      X: Math.random() * 100 - 50,
      Z: Math.random() * 100 - 50,
      point: undefined,
      Speed: undefined,
      IsDash: false
    }
  }

  emit(ws, data) {
    ws.send(JSON.stringify(data))
  }

  broadcast(sender, message) {
    for (let c of this.clients) {
      if (c.readyState === 1) {
        c.send(message)
      }
    }
  }

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
