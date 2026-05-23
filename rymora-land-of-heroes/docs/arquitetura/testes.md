# Testes - Rymora Land of Heroes

Data de criacao: 2026-05-23
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Stack

Testes do Core usam xUnit.

Comandos:

```powershell
dotnet test RymoraLandOfHeroes.sln
dotnet build RymoraLandOfHeroes.sln
```

Rider deve abrir `RymoraLandOfHeroes.sln` e descobrir os testes automaticamente.

---

## 2. Organizacao

Estrutura:

```text
src/Tests/
|-- Application/
|-- Combat/
|-- Hero/
|-- Party/
|-- World/
`-- ObjectMothers/
```

Cada dominio tem seus testes em pasta propria. ObjectMothers ficam centralizadas em `src/Tests/ObjectMothers/`.

---

## 3. Regra de Escopo

Cada teste deve verificar uma unica coisa.

Permitido:
- um comportamento por `[Fact]`.
- um metodo/operacao principal no Act.
- um resultado principal no Assert.

Evitar:
- testar varias regras no mesmo `[Fact]`.
- misturar setup complexo dentro do teste.
- asserts sobre estados secundarios que pertencem a outro comportamento.

Se uma regra precisa validar dois efeitos, criar dois testes com o mesmo ObjectMother ou dois cenarios separados.

---

## 4. ObjectMother

ObjectMother cria o cenario do teste.

O teste chama o metodo sendo testado. ObjectMother nao deve esconder o Act principal.

ObjectMother retorna um record de cenario contendo somente:
- `Input*`: entradas do metodo testado.
- `Assert*`: objeto ou chave usada para consultar resultado.
- `Expected*`: valor esperado no assert.

Exemplo:

```csharp
var scenario = InventoryObjectMother.AddItemWithSameNameAndLevel();

scenario.InputInventory.AddItem(scenario.InputItem);

Assert.Equal(
    scenario.ExpectedQuantity,
    scenario.InputInventory.GetQuantity(scenario.AssertItemName, scenario.AssertItemLevel));
```

Neste exemplo:
- ObjectMother prepara inventario com estado inicial.
- `AddItem` continua visivel no teste.
- Teste verifica somente agrupamento por nome e nivel.

---

## 5. Nomenclatura

Testes usam nomes descritivos em snake_case:

```csharp
[Fact]
public void AddItem_groups_items_by_name_and_level()
```

ObjectMothers usam sufixo `ObjectMother`:

```csharp
InventoryObjectMother
PartyObjectMother
WorldObjectMother
```

Cenarios usam sufixo `Scenario`:

```csharp
AddItemGroupingScenario
TravelUpdateScenario
```

---

## 6. Core Puro

Testes de Core nao podem depender de Godot.

Proibido em `src/Tests` para testes do Core:
- `Vector2I`
- `Node`
- `Resource`
- `Texture2D`
- qualquer namespace Godot

Adaptadores Godot terao testes proprios depois, separados dos testes do Core.
