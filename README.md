# Projeto de Testes Automatizados da Dog API (.NET)

Este projeto contém testes automatizados para a [Dog API](https://dog.ceo/dog-api/), desenvolvidos com .NET, NUnit, RestSharp e FluentAssertions.

## Tecnologias Utilizadas
* **.NET 8 (C#)**
* **NUnit:** Framework de testes
* **RestSharp:** Cliente HTTP para requisições de API
* **FluentAssertions:** Biblioteca de asserções
* **Allure.NUnit:** Integração do Allure com NUnit para relatórios de testes
* **Allure.Net.Commons:** Biblioteca base do Allure para .NET

---

## ⚙️ Configuração do Ambiente

**Pré-requisitos:**
1.  [.NET 8 SDK (ou 6/7)](https://dotnet.microsoft.com/download)
2.  Git

**Instalação:**
1.  Clone este repositório:
    ```bash
    git clone git@github.com:joserenatopera/dog-api-tests.git
    cd DogApiTests.Net
    ```
2.  Restaure as dependências do NuGet:
    ```bash
    dotnet restore
    ```

---

## 🚀 Execução dos Testes

### Execução Local
Para executar todos os testes, utilize o seguinte comando:

```bash
dotnet test
```

---

## 📊 Relatório Allure

O relatório dos testes automatizados é publicado automaticamente no GitHub Pages após cada execução da pipeline:

👉 [Acesse o relatório Allure aqui](https://joserenatopera.github.io/dog-api-tests/)

Você pode visualizar os resultados detalhados dos testes, cenários e evidências diretamente pelo navegador.