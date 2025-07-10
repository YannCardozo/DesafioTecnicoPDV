Desafio Técnico do PDV NET.

O Backend foi desenvolvido em c# utilizando o .net core 9.0.

Criação das camadas General (Model), Repository, Controller , Infra e PdvNetDesktop ( WinForms ).

A camada Model foram colocadas as DTOS e Responses, se distinguindo basicamente pelo atributo ID.
Response tem ID e DTO não tem ID.

Na camada Infra temos os DAOs que são as entidades do banco, permanecendo isoladas no Infra junto ao DB CONTEXT. Toda a parte de persistência dos dados e obtenção foi destinado a Infra.

Com utilização do Entity Framework para ORM, facilitando as criações do banco de dados e facilitando as querys de consultas ao banco utilizando LINQ. Entidades mapeadas MANUALMENTE usando o fluent para não deixar o EntityFramework tomar as decisões. Relacionamentos feitos e tabela Locatário Substituída pelo aspnetusers ( Users ) do identity.

Foram utilizados o Identity ( User, Roles ) como core da tabela de usuários, permissionamento através dos perfis e associações dos mesmos.

A Web Api tem 5 controladoras de casos de uso.

Mantido a tag \[Authorize] Para a maioria dos endpoints forçando a autenticação do usuário para consumir os endpoints, retornando 401 como Não autorizado em caso de consumir o determinado EndPoint sem o envio no header do TOKEN JWT.

Get All, Get by Id no Imóvel são desprotegidos propositalmente para permitir a exibição dos imóveis cadastrados no sistema web.

Create em Usuários e Login em Auth, também são desprotegidos justamente pela necessidade de não poderem estar autenticados a api para consumir eles.

Aluguel,Imovel,Usuario,Auth e Perfil

Aluguel -> 

-GetAll: Retornando todos os indícios de registros do caso de uso.

-GetAllById: Excepcionalmente em ALUGUEL temos esse endpoint para retornar TODAS as incidências de acordo com o ID que enviamos do token JWT no front, utilizado apenas no REACT.
-GetById ( ID ) : Retornando o registro especificado pelo ID da tabela, caso encontrado.
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisição.
-Create ( OBJ DTO ) : Criação de um novo OBJ a partir do obj enviado no corpo da requisição.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Imovel ->



-GetAll: Retornando todos os ind?cios de registros do caso de uso.
-GetById ( ID ) : Retornando o registro especificado pelo ID da tabela, caso encontrado.
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisi??o.
-Create ( OBJ DTO ) : Cria??o de um novo OBJ a partir do obj enviado no corpo da requisi??o.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Usuário ->

-GetAll: Retornando todos os ind?cios de registros do caso de uso.
-GetById ( ID ) : Retornando o registro especificado pelo ID da tabela, caso encontrado.
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisi??o.
-Create ( OBJ DTO ) : Cria??o de um novo OBJ a partir do obj enviado no corpo da requisi??o.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Perfil ->
-GetAll: Retornando todos os ind?cios de registros do caso de uso.
-AssociarPerfil: Associa Perfil a determinado USUARIO para incluir em UserRoles (Identity)
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisi??o.
-Create ( OBJ DTO ) : Cria??o de um novo OBJ a partir do obj enviado no corpo da requisi??o.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Auth -> sendo responsável pela AUTENTICAÇÃO do Usuário, sendo o LOGIN realizado por lá, gerado o token JWT e respectivos Claims do token.


Implementado autenticação básica com o token JWT, disponibilizando CLAIMS no Token para obtenção delas no front end. Inserido o token JWT no COOKIES por oferecer maior SEGURANÇA, pela variável "pdvnet\_token"


#Winforms
O Winforms foi dividido em componentes para algumas reutilizações de UI, utilitarios para métodos, sessão para obtenção do token JWT e envio as requisicoes de endpoints, para poder consumir com a autorização necessária. Resources com Logo da tela de Login e Loader de gif carregando para dar interatividade ao usuário do sistema durante os carregamentos do GRID.

O DataGrid foi a escolha para utilização do CRUD

