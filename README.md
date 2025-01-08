# **Gestión de Parqueaderos API**

Este proyecto es una API desarrollada con **ASP.NET Core**, diseñada para gestionar y controlar la información de parqueaderos, vehículos, y el registro (histórico) de entradas y salidas de vehículos.

## **Requisitos Previos**

Antes de comenzar, asegúrate de tener lo siguiente instalado en tu máquina local:

- **[.NET SDK 8.0 o superior](https://dotnet.microsoft.com/download)**  
- **[SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads)** o cualquier base de datos que el proyecto utilice (configurable en el archivo `appsettings.json`).  
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** o **[Visual Studio Code](https://code.visualstudio.com/)** con extensiones de .NET y C#.  
- **Git** para clonar el repositorio.  

## **Configuración Inicial**

### **1. Clonar los Repositorios**
Clona los repositorios en tu máquina local:  
```bash
git clone https://github.com/elprogramador2024/ParqueaderoAPI
cd ParqueaderoAPI

git clone https://github.com/elprogramador2024/EmailAPI
cd EmailAPI
```

##ParqueaderoAPI

### **2. Configurar la Base de Datos**
1. Asegúrate de que tu servidor de base de datos esté corriendo.
2. Configura la cadena de conexión en el archivo `appsettings.json`:  

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=NOMBRE-EQUIPO;Database=Parqueadero;Integrated Security=True;Encrypt=False"
}
```

### **3. Aplicar las Migraciones**
Ejecuta los siguientes comandos (en Consola de Administrador de Paquetes en Visual Studio) para aplicar las migraciones (ParqueaderoAPI) y crear la base de datos:  

```bash
PM> add-migration MigracionInicial
PM> update-database
```

### **4.Ejecutar el Proyecto en Local**

1. Abre los proyectos EmailAPI y ParqueaderoEmail en tu Visual Studio 
2. Selecciona el perfil de inicio deseado (por defecto: `IIS Express`).  
3. Ejecuta ambos proyectos

4. La APIs estarán disponible en:

- ParqueaderoAPI: **https://localhost:7245** (HTTPS)
- EmailAPI: **https://localhost:7076** (HTTPS)

### **5.Descarga y copia la colección de POSTMAN, con cada request documentado**

https://github.com/elprogramador2024/ParqueaderoAPI/blob/master/ParqueaderoAPI/Resources/swagger.json
