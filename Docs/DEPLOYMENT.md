# 🚀 Guía de Despliegue

## Tabla de Contenidos
- [Prerequisitos](#prerequisitos)
- [Recursos Azure](#recursos-azure)
- [Configuración de Seguridad](#configuración-de-seguridad)
- [Configuración de AzCopy](#configuración-de-azcopy)
- [Despliegue de Functions](#despliegue-de-functions)
- [Configuración de Data Factory](#configuración-de-data-factory)
- [Base de Datos SQL](#base-de-datos-sql)
- [Validación](#validación)

---

## Prerequisitos

### Local
- ✅ .NET 6.0 SDK
- ✅ Azure CLI
- ✅ Azure Functions Core Tools v4
- ✅ SQL Server Management Studio (SSMS)
- ✅ AzCopy v10

### Azure
- ✅ Suscripción activa de Azure
- ✅ Permisos de Contributor en Resource Group
- ✅ Cuota disponible para Azure OpenAI

---

## Recursos Azure

### 1. Crear Resource Group

```bash
az login

az group create \
  --name rg-call-analysis-prod \
  --location eastus