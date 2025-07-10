# Desafio Técnico do PDV NET.

O Backend foi desenvolvido em c# utilizando o .net core 9.0. 2 Sistemas diferentes ( Desktop e WEB ) consumindo a MESMA API com MESMA BASE DE DADOS. O caso de uso do login foi desenvolvido pensando em permitir o usuário se logar com o CPF ou o email, junto com a senha para facilitar e tornar a experiência do usuário melhor.

Dados para acesso aos sistemas,

cpf:25476523412  
email:pdvnet@pdvnet.com.br  
senha:PDVnet123!@  

URLS:  
-WEB API: https://testepdvnet.runasp.net/index.html  
-Sistema Web REACT: https://testepdvnet.netlify.app/  

# Arquitetura
Criação das camadas General (Model), Repository, Controller , Infra e PdvNetDesktop ( WinForms ).

A camada Model foram colocadas as DTOS e Responses, se distinguindo basicamente pelo atributo ID.
Response tem ID e DTO não tem ID.

Na camada Infra temos os DAOs que são as entidades do banco, permanecendo isoladas no Infra junto ao DB CONTEXT. Toda a parte de persistência dos dados e obtenção foi destinado a Infra.

Com utilização do Entity Framework para ORM, facilitando as criações do banco de dados e facilitando as querys de consultas ao banco utilizando LINQ. Entidades mapeadas MANUALMENTE usando o fluent para não deixar o EntityFramework tomar as decisões. Relacionamentos feitos e tabela Locatário Substituída pelo aspnetusers ( Users ) do identity.

Foram utilizados o Identity ( User, Roles ) como core da tabela de usuários, permissionamento através dos perfis e associações dos mesmos.

# Web API

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

-GetAll: Retornando todos os indícios de registros do caso de uso.
-GetById ( ID ) : Retornando o registro especificado pelo ID da tabela, caso encontrado.
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisição.
-Create ( OBJ DTO ) : Criação de um novo OBJ a partir do obj enviado no corpo da requisição.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Usuário ->

-GetAll: Retornando todos os indícios de registros do caso de uso.
-GetById ( ID ) : Retornando o registro especificado pelo ID da tabela, caso encontrado.
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisição.
-Create ( OBJ DTO ) : Criação de um novo OBJ a partir do obj enviado no corpo da requisição.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Perfil ->

-GetAll: Retornando todos os indícios de registros do caso de uso.
-GetById ( ID ) : Retornando o registro especificado pelo ID da tabela, caso encontrado.
-Update ( Obj Response ) : Enviando o Obj para Atualizar pelo Body da requisição.
-Create ( OBJ DTO ) : Criação de um novo OBJ a partir do obj enviado no corpo da requisição.
-Delete ( ID ) : ID a ser deletado enviado especificado pelo ID da tabela, caso encontrado.

Auth -> 
-Sendo responsável pela AUTENTICAÇÃO do Usuário, sendo o LOGIN realizado , gerado o token JWT e respectivos Claims do token.


Implementado autenticação básica com o token JWT, disponibilizando CLAIMS no Token para obtenção delas no front end. Inserido o token JWT no COOKIES por oferecer maior SEGURANÇA, pela variável "pdvnet\_token"


# Winforms

Será disponibilizado uma versão RELEASE do Build do aplicativo do WinForms que poderá ser utilizado pelo usuário ( após ser instalado o runtime do .net 9 para desktop windows, caso não tenha ele ainda. )

No ambiente do visual Studio, assim que acessar, entrar no menu acima "Compilação" e pressionar compilar solução, recompilar e limpar solução. Verificar se os pacotes e dependencias foram todos instalados corretamente. Após isso proceder com a utilização do winforms.

O Winforms foi dividido em componentes para algumas reutilizações no desenvolvimento da UI, utilitarios para métodos, sessão para obtenção do token JWT e envio as requisicoes de endpoints, para poder consumir com a autorização necessária. Resources com Logo da tela de Login e Loader de gif carregando para dar interatividade ao usuário do sistema durante os carregamentos do GRID.

A aplicação inicializa no LoginForm como tela INICIAL e em caso de login bem sucedido redirecionará para o FormPrincipal, onde terão os grids de aluguel, imovel, usuários e perfis.

O DataGrid foi a escolha para utilização do CRUD, possuem FILTROS que podem ser utilizados conforme a necessidade do usuário e irão retornar dados das colunas onde encontrar indícios do que foi pedido. Respeitado todas as operações dos CRUDS e sendo feitas também as operações. No WinForms está sendo permitido o envio de imagens dos IMOVEIS no caso de uso do Imovel, e armazenado em string base 64 caso o usuário deseje realizar isso, tanto no update como no create é possível. Por ser NULLABLE, não é necessário preencher.

O usuário pode ter um perfil, pode cadastrar um imovel, criar um aluguel mediante a associação de usuário com imovel.

# Sistema Web - React

Sistema Web React acessível no link do netlify, com deploy disponível. 
