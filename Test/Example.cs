namespace MinimalAPI_Test
{
    /* ======== Como funciona as classes e métodos de teste ========
     
    Uma classe de teste é uma classe que contém métodos de teste, e cada método de teste é responsável por testar um aspecto específico do código.

    Mas a instanciação e configuração do ambiente de teste, como a criação de objetos, configuração de dependências, etc., 
        é feita dentro do método [ClassInitialize], que é executado uma vez antes de todos os testes da classe.

    E podemos para cada instância de teste, criar um método [TestInitialize], 
        que é executado antes de cada teste, para configurar o ambiente de teste específico para aquele teste.

    Esses dois tipos de métodos são importantes, pois para cada [TestMethod] é criado uma nova instância da classe de teste.

    Exemplo de execução de teste:

    --- [TestInicialize] ---    
    new Exemplo()
    -> TestInitialize()
    -> TestMethod1()
    -> (instância descartada)
    -> new Exemplo()
    -> TestInitialize()
    -> TestMethod2()

    --- [ClassInitialize] ---
    Setup()
    -> new Exemplo()
    -> TestInitialize()
    -> TestMethod1()
    -> (instância descartada)
    -> new Exemplo()
    -> TestInitialize()
    -> TestMethod2()

    Por isso, o método [ClassInitialize] tem que ser:
        1. Estático (Static), pois ele não depende de uma instância da classe e é executado antes de qualquer instância ser criada.
        2. Privado (Private), pois ele é um método de configuração interna da classe de teste e não deve ser acessível fora da classe.
        3. Deve receber um parâmetro do tipo TestContext, que é um objeto fornecido pelo framework de teste para armazenar informações sobre o ambiente de teste e os resultados dos testes.


     */

    /* === Banco de dados em ambiente de teste ===
     
    Você quer:
        • Projeto puramente de teste.
        • Banco descartável.
        • Ambiente previsível.

    Então a pergunta chave é:
        | O que deve acontecer antes de cada teste? 
        | E o que deve acontecer apenas uma vez?
     
    🧠 Modelo mental correto

    Criar DATABASE (nível servidor)
        → operação pesada
        → deve acontecer uma vez
        → ClassInitialize

    Criar SCHEMA (tabelas)
        → pode acontecer uma vez
        → ou pode acontecer por teste

    Criar CONTEXTO
        → deve acontecer por teste
        → TestInitialize ou dentro do método

    --- Agora pense em isolamento

    Se você tem 5 testes que inserem dados:

        Teste A cria usuário
        Teste B também cria usuário

    Se você não limpar o banco, o Teste B pode falhar porque já existe registro.

    • Estratégia 1 — Recriar banco inteiro por teste

        Mais isolado
        Mais lento

    → usar <<EnsureDeleted()>> + <<EnsureCreated()>> em [TestInitialize]
    ---------------------------------------------------------------------
    Estratégia 2 — Criar banco uma vez e limpar tabelas

        Mais rápido
        Mais controle manual
    ---------------------------------------------------------------------
    Estratégia 3 — Usar transação e dar rollback

        Mais elegante
        Mais avançado


    ---- O fluxo mental correto para seu cenário

    Como é projeto descartável:

    1. [ClassInitialize]

        • Criar database se não existir
        • Recriar schema limpo

    2. Cada teste:

        • Criar novo DbContext
        • Executar serviço
        • Validar resultado

    Pergunta para refletir:

    Por que criar novo <<DbContext>> por teste é importante?

    Resposta:
        Porque <<DbContext>> mantém estado interno (ChangeTracker).
        Se reutilizar, os testes começam a interferir entre si.


    8️⃣ Resumo conceitual

    [ClassInitialize]
        → Setup pesado
        → Infraestrutura compartilhada
        → Executa 1 vez

    [TestInitialize]
        → Setup leve
        → Estado isolado
        → Executa antes de cada teste

    Banco de teste
        → Deve ser exclusivo
        → Deve ser descartável
        → Nunca compartilhar com ambientes reais
     */

    // [TestClass]
    public sealed class Example
    {
        private static string _connectionString = string.Empty;

        // [ClassInitialize]
        private static void Setup(TestContext context)
        {
            // Dentro desse método [ClassInitialize], é onde você pode configurar o ambiente de teste, 
            // como criar objetos, configurar dependências, etc., que serão usados por todos os testes da classe.
        }

        // [TestMethod]
        public void TestMethod1()
        {
        }

        // [TestMethod]
        public void TestMethod2()
        {
        }
    }
}
