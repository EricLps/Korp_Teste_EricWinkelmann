# Korp_Teste_EricLopesWinkelmann

## Descrição
O projeto consiste em um sistema distribuído para emissão de Notas Fiscais e controle de estoque, construído utilizando a arquitetura de microsserviços. O sistema garante que notas fiscais só sejam emitidas se houver saldo disponível em estoque, efetuando uma reserva temporária otimista e assíncrona.

## Tecnologias
- Backend: C# .NET 8, ASP.NET Core Web API, Entity Framework Core, PostgreSQL
- Frontend: Angular 17, TypeScript, Angular Material

## Pré-requisitos
- .NET 8.0 SDK ou superior
- Node.js v18.0 ou superior
- Angular CLI 17+ (`npm install -g @angular/cli`)
- PostgreSQL 15+ rodando na porta 5432 (ou Docker com a imagem do Postgres)

## Configuração do banco de dados
Certifique-se de que o PostgreSQL está rodando com o usuário `postgres` e senha `postgres` na porta `5432` (configuração padrão nos arquivos `appsettings.json`).
Não é necessário rodar scripts SQL manualmente. Os bancos `korp_stock` e `korp_billing` e suas respectivas tabelas são criados automaticamente através do Entity Framework Core Migrations quando as aplicações são iniciadas (ou rodando o comando de database update).

## Rodando o StockService
1. Abra um terminal na pasta `StockService/StockService`.
2. Opcional: Se for a primeira vez, aplique as migrations com `dotnet ef database update`.
3. Inicie o microsserviço executando: `dotnet run`.
4. A API ficará disponível em `http://localhost:5001`.

## Rodando o BillingService
1. Abra um terminal na pasta `BillingService/BillingService`.
2. Opcional: Se for a primeira vez, aplique as migrations com `dotnet ef database update`.
3. Inicie o microsserviço executando: `dotnet run`.
4. A API ficará disponível em `http://localhost:5002`.

## Rodando o Frontend
1. Abra um terminal na pasta `frontend`.
2. Instale as dependências executando: `npm install`.
3. Inicie a aplicação Angular executando: `ng serve`.
4. Acesse pelo navegador em: `http://localhost:4200`.

## Detalhamento técnico
- **Ciclos de vida Angular utilizados e onde:** 
  Utilizamos primariamente o `ngOnInit` em `ProductsComponent` e `InvoicesComponent` para inicializar a busca de dados nas APIs assim que os componentes são renderizados na tela (ex: `this.loadProducts()`), garantindo que as tabelas já venham preenchidas.
  
- **Como o RxJS foi utilizado:**
  O RxJS foi utilizado nos `Services` do Angular para lidar com requisições HTTP assíncronas através de `Observables`. Utilizamos operadores como `catchError` para interceptar e tratar falhas de comunicação com a API de forma graciosa no frontend, e o operador `finalize` para garantir que a variável de estado `isLoading` retorne para `false` após o término da requisição, independentemente de ter sido um sucesso ou um erro.

- **Bibliotecas de UI e finalidade:**
  Utilizamos o **Angular Material** para garantir consistência visual e acessibilidade. Seus componentes foram essenciais: `MatTableModule` para as listagens de dados, `MatFormField` e `MatInput` para a construção de formulários reativos consistentes, `MatSnackBar` para os feedbacks visuais interativos na base da tela (toast notifications), e `MatIcon` para os botões de ações, como a impressora.
  
- **Como o LINQ foi utilizado no C#:**
  O LINQ foi amplamente utilizado nas camadas de `Service` e `Repository` para consultas em memória e no banco de dados de forma declarativa. Exemplos notáveis incluem `.FirstOrDefault()` para buscar a ocorrência única de uma reserva existente na validação de Idempotência, e `.Where().Sum()` para somar a quantidade total de reservas ativas de um produto de maneira rápida e segura.
  
- **Como erros e exceções são tratados no backend:**
  Foi evitado o uso de retornos silenciosos. As camadas de `Service` (regras de negócio) disparam `InvalidOperationException` ou `KeyNotFoundException`. A camada de controle (`Controllers`) intercepta essas exceções específicas e as converte no padrão HTTP adequado (ex: Status 400 ou 404) embutindo as mensagens originais na estrutura padronizada de resposta `ProblemDetails`, que é perfeitamente compreendida pelo Angular. Falhas de comunicação entre os microsserviços geram um `HttpRequestException`, interceptado para não expor a stack de erro ao cliente.
  
- **Explicação da arquitetura de reserva de estoque com prazo de 24h:**
  Para evitar que notas não finalizadas prendam o estoque para sempre, usamos o padrão de Reserva de Estoque. O `BillingService` cria uma `StockReservation` temporária (Status: Active). Se a nota não for impressa (fechada) em 24h, um Background Service agendado que varre o banco limpa as reservas expiradas automaticamente e devolve o saldo de forma assíncrona, sem sobrecarregar as requisições principais de leitura.
  
- **Como a concorrência é tratada:**
  Considerando que dois clientes poderiam tentar emitir uma nota para o último item do estoque no exato mesmo milissegundo, implementamos um bloqueio via *Optimistic Concurrency* no Entity Framework Core. Uma anotação `[ConcurrencyCheck]` foi adicionada à propriedade `Balance` do produto. Dessa forma, caso haja colisão de atualização simultânea, o Entity Framework nativamente barra a segunda transação através de uma `DbUpdateConcurrencyException`, impedindo que o estoque fique negativo, sem a necessidade de travar as tabelas inteiras no PostgreSQL.
