# Proyecto: Simulación Cliente-Servidor Multijugador (Host / Client)

## Descripción General
Este proyecto implementa una **simulación de red multijugador en Unity**, compuesta por dos roles principales:

- **Host**: actúa como servidor TCP, recibiendo y retransmitiendo datos entre los jugadores conectados.  
- **Client**: se conecta al host y sincroniza su posición, rotación y estado con el resto de jugadores.

El sistema permite ejecutar múltiples clientes conectados a un único host local, simulando una arquitectura de red básica para análisis, pruebas o prácticas de programación de sockets.

---

## Versión del Proyecto
- **Motor:** Unity 6000.2.7f2  
- **Lenguaje:** C#  
- **Tipo de comunicación:** TCP (sincrónica, no encriptada)  
- **Plataformas compatibles:** Windows, macOS, Linux  

---

## Estructura General del Proyecto

### 1. Escenarios disponibles
- **HostScene** → se ejecuta como servidor.  
- **ClientScene** → se ejecuta como cliente conectado.  

Cada escena tiene su propio sistema de menús, controladores y lógicas de red.

---

### 2. Scripts principales

| Tipo | Script | Función principal |
|------|---------|-------------------|
| Host | `TCPServer` | Crea y gestiona el servidor TCP. Acepta conexiones y retransmite datos. |
| Host | `HostDisconnectController` | Permite apagar el servidor y desconectar a todos los clientes. |
| Client | `TCPClient` | Conecta al servidor, envía y recibe datos de posición. |
| Client | `ClientPlayer` | Controla el movimiento del jugador local. |
| Común | `RemotePlayersManager` | Sincroniza las posiciones de los demás jugadores. |
| Común | `MenuInicial` | Permite elegir entre Host o Cliente. |
| Común | `PauseMenuHost` / `PauseMenuClient` | Controlan las funciones del menú de pausa. |

---

## Instrucciones de Ejecución

### **1. Configurar el proyecto**
1. Abre el proyecto en **Unity 6000.2.7f2** o una versión superior compatible.  
2. Asegúrate de tener habilitado el módulo **.NET Scripting Backend**.  
3. Verifica que las escenas `HostScene` y `ClientScene` estén añadidas al **Build Settings**.

---

### **2. Ejecución como Host**
1. Abre la escena `MenuHost`.  
2. Ejecuta el juego desde Unity o genera una **Build** (Recomendado: Windows/Mac).  
3. El host mostrará su dirección IP local.  
4. Los clientes deben usar esa IP para conectarse.  
5. Desde el **menú de pausa del host**, el botón **“Desconectar Servidor”** cerrará todas las conexiones activas.

---

### **3. Ejecución como Cliente**
1. Abre la escena `MenuClient`.  
2. Ingresa la **IP del Host** y presiona “Conectar”.  
3. Al conectarse, el cliente podrá moverse y ver reflejadas las posiciones de los demás jugadores.  
4. Si el host se desconecta, el cliente será desconectado automáticamente.

---

## Flujo de Comunicación (Resumen Técnico)
1. El **Host** abre un servidor TCP mediante `TCPServer`.  
2. Los **Clientes** se conectan al servidor usando `TCPClient`.  
3. Cada cliente envía su posición al servidor.  
4. El servidor retransmite esos datos al resto de clientes mediante `ServerRebroadcaster`.  
5. `RemotePlayersManager` en cada cliente actualiza las posiciones remotas en la escena.  
6. Si el host cierra sesión, se detiene el servidor y todos los clientes se desconectan.

---

## Recomendaciones
- Ejecutar el host **antes** de conectar los clientes.  
- Todos los equipos deben estar en la **misma red local**. 