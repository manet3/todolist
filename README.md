# TodoList Client & Server
Cílem je vytvořit jednoduchý todo list a to jak klientskou aplikaci (ve WPF), tak server (v c#).

## Obecné

### Git
 - Necommituj přímo do **master** a **devel**
 - Na každou feature založ z develu novou větev např. **feature/new_ui**.
 - Ve větvi **master** má být vždy jen kód určený k releasu (otestovaný a který prošel code review)
 - Větev **devel** obsahuje aktuální vývojovou verzi, všechen kód, který se do ní dostane by měl být zamergovaný z jiné větve a projít code review.
 - Ostatní větve jsou vytvořené z develu a obsahují aktuální rozpracovaný kód k dané funkci/bugfixu atd.., na kterém se zrovna pracuje. Na feature branchích většinou pracuje jen jeden člověk a je povolené do nich force pushovat (tj. měnit historii).
 - Když dokončíš práci na nějaké feature, otevři **merge request** dané freature branche do **develu**. Než bude zamergována, projde code review.

## Fáze 1 - Client
![Capture](/uploads/33df90f0c855a05d6512416fae2832b9/Capture.PNG)
 - Založ ve visal studiu nový solution (ToDoList) a v něm projekt (ToDoList.Client).
 - Vytvoř ve WPF UI dle obrázku. Snaž se rozumět (a ideálně přímo piš) XAMLu, určitě budeš potřebovat věci jako Grid, StackPanel, ListView, TextBox, TextBlock...
 - Tlačítko **+** přidá nový úkol do seznamu a vymaže vstupní pole.
 - Propoj UI s "kódem" pomocí data bindigu. Zatím můžeš použít i codebehind.

## Fáze 2 - Server
 - Přidej do solution další projekt, který bude implementovat http server (buď můžeš použít konzolovou app a HTTPListener nebo udělat jednoduchou ASP.NET aplikaci).
 - Server bude poskytovat REST API pro klientskou aplikaci a ukládat data do databáze. Bude mít tyto endpointy:
 - /List - GET request vrátí JSON obsahující všechny úkoly
 - /Add - POST request s názvem nového úkolu vytvoří na serveru nový úkol.
 - /Change - PUT/PATCH request umožňující aktualizovat to jeslti je úkol hotový nebo ne.
 
## Fáze 3 - propoj Client a Server
 - Aplikace bude synchonizována se serverem, všechna volání volání severu by měla být neblokující.
 - Jak se bude aplikace chovat, když nepůjde internet?

## Fáze 4 - lepší client
 - použij MVVM, Caliburn, Fody, Json.NET, případně stylování app, další obrazovky...

## Fáze 5 - lepší server
 - použij NancyFx, možnost rozšíření úkolů třeba o tagy...