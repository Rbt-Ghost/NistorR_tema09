# Întrebări și Răspunsuri (Reformulate)

### 1. Utilizați pentru texturare imagini cu transparență și fără. Ce observați?

La utilizarea imaginilor fără transparență (ex. JPG), textura este complet opacă și acoperă integral suprafața obiectului, în timp ce imaginile cu transparență (ex. PNG), folosite împreună cu funcția de amestecare a culorilor (Alpha Blending), permit vizualizarea obiectelor din plan secund prin zonele transparente ale texturii.

### 2. Ce formate de imagine pot fi aplicate în procesul de texturare în OpenGL?

Deși OpenGL lucrează cu date brute, prin intermediul bibliotecilor de încărcare (precum `System.Drawing` din .NET utilizată în acest proiect) se pot folosi formate standard de imagine cum ar fi BMP, JPG (pentru imagini opace) și PNG (esențial pentru transparență), care sunt apoi convertite în texturi compatibile cu placa video.

### 3. Specificați ce se întâmplă atunci când se modifică culoarea (prin manipularea canalelor RGB) obiectului texturat.

Atunci când se modifică culoarea obiectului, aceasta acționează ca un filtru de nuanțare (tinting) peste textură, deoarece culoarea finală a fiecărui pixel este rezultatul matematic al înmulțirii culorii din imaginea texturii cu valorile RGB definite pentru obiectul respectiv.

### 4. Ce deosebiri există între scena ce utilizează obiecte texturate în modul iluminare activat, respectiv dezactivat?

Când iluminarea este dezactivată, obiectele texturate apar plate și uniform luminate la intensitate maximă (ca un desen 2D), în timp ce activarea iluminării conferă profunzime și volumetrie scenei, deoarece texturile reacționează la sursele de lumină generând zone de umbră difuză și reflexii speculare pe suprafața obiectelor.
