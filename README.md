# A szoftver használata:
## Első futtatás előtt
Először az adatbázisokat fel kell húzni, és a frontend packageket telepíteni kell. 

Ezekhez adjuk ki a gyökérmappából az alábbi parancsot:
```
./updatedb.ps1
```

Majd a ContractViewerClient mappából:
```
npm install
```

## Indítás
A backend indításához a gyökérmappából kell futtatni az alábbi parancsot:
```
./startbe.ps1
```

A frontend indításához a ContractViewerClient mappába navigálva adjuk ki az alábbi parancsot:
```
npm start
```

## Főbb folyamatok
A POC kipróbálásához be kell jelentkezni. Az előre definiált user-ek a `ContractViewerApi/UserApi/IdentitySeeder.Users.cs` file-ban találhatók. 

Bejelentkezéshez navigáljunk a böngészőben a localhost:4200/login oldalra, majd töltsük ki a login formot! A jelszó minden usernek Admin_123.

Bejelentkezés után a cache-be kerül a bejelentekezett userhez tartozó user id (Pontosabban a bejelentkezést követő első olyan művelet végrehajtásakor, amihez kell a user id). Ezt meg is tudjuk nézni a cache editorban, a jobb felső sarokban. Itt tetszőlegesen tudunk törölni cache adatokat és frissíteni a cache-t. 

## Contractok
A bejelentkezett felhasználó szerződései lekérdezésre kerülnek a `/contracts` oldal megnyitásakor. 

Ha a contractok az adott userhez már a cache-ben vannak, akkor ezeket jeleníti meg.

Ha a contractok nincsenek a cache-ben, de a user id igen, akkor az app ezt fogja használni a szerződések lekérdezésére. Ha egyik sincs, akkor le fogja kérdezni a bejelentkezett user id-ját, majd beteszi a cache-be, és ezt fogja használni a szerződések lekérdezésére, és azok is bekerülnek a cache-be. 

Az adott user-hez lehet véletlenszerűen generálni szerődéseket, hogy könnyen fel lehessen tölteni adattal. Ehhez, a '+' gombra kell rányomnunk a szerződések listázásakor.

## Dokumentumok
A `/documents` oldalon lehet lekérdezni az adott felhasználó összes szerződéséhez tartozó dokumentumot.

A dokumentumokat szerződés id-k listája alapján lehet lekérdezni.

Ehhez a backend lekérdezi az összes szerződést, ami a bejelentkezett userhez tartozik, majd ezek alapján keres a dokumentumokban. A cache-elési logika itt is érvényes, tehát ha a contractok az adott userben már benne vannak a cache-ben akkor azt használja, ha nem, akkor a fent leírt módon jár el a rendszer.

A dokumentumok a lekérdezésük után szintén bekerülnek a cache-be.

Az adott user-hez dokumentumokat is lehet véletlenszerűen generálni. Ehhez, a '+' gombra kell rányomnunk a dokumentumok listázásakor.
