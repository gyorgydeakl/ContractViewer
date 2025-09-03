Ez a demo egy POC arra, hogy egy Redis vagy azzal kompatibilis cache használata hogyan képzelhető el egy cég-specifikus rendszerben. 
A demo bemutatja egyszerre a Redis, a Valkey és a KeyDB használatát.
Redis
    - ismét ingyenes az alap verzió + fizetni lehet felsőbbi tierekért
    - sokan azt írják, hogy már nem annyira megbízható a cég, bármikor fizetőssé válhat ismét
Redis forkok:
KeyDB
    - Snap Inc. (snapchat) tulajdonában
    - Gyenge supportáltság
    - nincs fizetős support.
    - Git repo-n kevés megoldott issue (30/160 2 év alatt)
    - utolsó verziót (6.3.4) 2023 október 30-án adták ki, azóta a fejlesztés vélhetően áll
    - A két legfőbb hozzájáruló sem aktív már egy ideje
    - JohnSully felhasználó közölte is az év elején, hogy elhagyja a projektet.
Valkey
    - Linux foundation áll mögötte
    - GitHub oldaluk sokkal aktívabb, 
    - Utolsó verzió 2025 Június
    - Még nem képes ezekre:
    - full multithreading
    - multi-master
    - FLASH

# A szoftver indítása:
## Első futtatás előtt
- Szükséges, hogy fusson egy Redis (vagy egyéb kompatibilis process) instance a localhost:6379-on és egy SQL Server instance.
    - A redis futtatásához javaslom a docker-t. Egy előre bekonfigurált instance-et tudunk futtatni, ha az alábbi parancsot kiadjuk a gyökérmappában 
        ```powershell
        docker compose up -d
        ```
        - Az, hogy milyen host-on keresse a cache-t (jelenleg localhost), az állítható a `ContractViewerApi/ContractViewerApi/appsettings.json` mappában a `ConnectionStrings` rész alatt.
    - Az SQL Server-hez pedig ajánlom a SQL Server Management Studio telepítését.
        - Minden adatbázis connection stringje (és ezáltal akár az SQL Server szükségessége) állítható az adott alkalmazás `appsettings.json` config-jában 
- Az adatbázisok felhúzása: Adjuk ki a gyökérmappából az alábbi parancsot:
    ```
    ./updatedb.ps1
    ```
- A frontend packagek telepítése. Adjuk ki a `ContractViewerClient` mappából az alábbi parancsot:
    ```
    npm install
    ```

## Indítás
A backend indításához először buildelni kell az egész solution-t, majd a gyökérmappából kell futtatni az alábbi parancsot:
```
./startbe.ps1
```

A frontend indításához a ContractViewerClient mappába navigálva adjuk ki az alábbi parancsot:
```
npm start
```
