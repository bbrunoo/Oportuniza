
# Oportuniza
<div align="justify">
Oportuniza é uma plataforma web desenvolvida para conectar pessoas a oportunidades de trabalho essenciais. O sistema permite que empresas e usuários comuns publiquem vagas ou solicitem serviços em áreas como babá, repositor, cuidador de idosos, eletricista, entre outras funções do dia a dia. A proposta é facilitar o acesso a empregos e serviços de forma simples, ágil e acessível para todos.


## Estrutura do Projeto

* Oportuniza.API: API em C# (.NET), responsável pelo backend.

* Oportuniza.Application: Frontend Angular (SPA responsiva).

## Requisitos

### Backend (API .NET)

* .NET SDK 6.0 ou superior

* SQL Server

* Entity Framework CLI (dotnet tool install --global dotnet-ef)

* Visual Studio ou Visual Studio Code

Frontend (Angular)
* Node.js (v16 ou superior)

* Angular CLI (npm install -g @angular/cli)

#

### Como rodar o projeto
## Backend (.NET API)
   1. Configurar o `appsettings.json`

2. No arquivo `Oportuniza.API/appsettings.json`, substitua os valores das chaves do Azure e da ConnectionString com suas credenciais locais. Essas chaves são privadas e não estão incluídas no repositório por segurança.

3. Atualizar o banco de dados com Entity Framework

4. No terminal, dentro da pasta `Oportuniza.API`, execute o comando abaixo para aplicar as migrations e criar/atualizar o banco de dados conforme os modelos definidos no projeto.

``` Bash
dotnet ef database update
```

5. Executar a API 

* Ainda no mesmo terminal (na pasta `Oportuniza.API`), inicie o servidor da API com o comando:

```Bash
dotnet run
```

A API estará em execução e pronta para receber requisições, geralmente no endereço `https://localhost:5001` ou `http://localhost:5000`.

---
## Frontend (Angular)

1. Navegar para a pasta do projeto

2. Abra um novo terminal e vá para a pasta do frontend:

```Bash
cd Oportuniza.Application
```
3. Instalar as dependências

* Execute o comando abaixo para instalar todas as bibliotecas e pacotes necessários para o projeto Angular.


```Bash
npm install
```
4. Executar a aplicação

Após a instalação das dependências, inicie o servidor de desenvolvimento do Angular com:

```Bash
ng serve
```

A aplicação front-end estará acessível no seu navegador através do endereço http://localhost:4200/. O servidor de desenvolvimento recarregará a página automaticamente sempre que você fizer alterações nos arquivos do projeto.
</div>
