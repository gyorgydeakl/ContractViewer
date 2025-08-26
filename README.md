Ez a demo egy POC arra, hogy egy redis vagy azzal kompatibilis cache használata hogyan képzelhető el egy cég-specifikus rendszerben 
# A szoftver indítása:
## Első futtatás előtt
- Szükséges, hogy fusson egy Redis (vagy egyéb kompatibilis process) instance a localhost:6379-on és egy SQL Server instance.
    - A redis futtatásához javaslom a docker-t, mindössze 1 parancs futtatásával el tudunk indítani egy instance-et.
        ```powershell
        docker run --name redis-poc -d -p 6379:6379 redis
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
A backend indításához a gyökérmappából kell futtatni az alábbi parancsot:
```
./startbe.ps1
```

A frontend indításához a ContractViewerClient mappába navigálva adjuk ki az alábbi parancsot:
```
npm start
```