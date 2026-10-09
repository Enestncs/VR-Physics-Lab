# VR Physics Lab — Game Design Document (GDD)

> **Belge türü:** İlk oynanabilir prototip için Game Design Document  
> **Sürüm:** 0.1  
> **Durum:** Taslak  
> **Hedef geliştirme süresi:** 8–12 hafta  
> **Hedef platform:** Meta Quest 3  
> **Geliştirme ortamı:** Unity + Quest Link  
> **Hedef kitle:** 9–10. sınıf öğrencileri

---

## 1. Proje Özeti

**VR Physics Lab**, lise öğrencilerinin temel mekanik konularını deneyerek keşfetmesini amaçlayan, sanal gerçeklik tabanlı ve oyunlaştırılmış bir fizik laboratuvarı prototipidir. Oyuncu tek bir laboratuvar alanında fizik düzenekleri kurar, deney parametrelerini değiştirir, sonuçları gözlemler ve tahminleriyle karşılaştırır.

Proje, kapsamlı bir eğitim ürününden önce portfolyoda sergilenebilecek, baştan sona oynanabilir bir **vertical slice** üretmeyi hedefler.

### 1.1 Yüksek konsept

Oyuncu sanal bir fizik laboratuvarında arabalar, ağırlıklar, yüzeyler ve rampalarla deney yapar. Her deneyde önce tahminde bulunur, ardından düzeneği kurar, sonucu gözlemler ve fiziksel açıklamayla öğrenmesini pekiştirir.

### 1.2 Tasarım ilkeleri

- **Deneyerek öğrenme:** Fizik kavramları yalnızca metinle anlatılmaz; oyuncu bunları deneyimleyerek keşfeder.
- **Oyuncu kontrolü:** Öğrenci parametreleri değiştirebilir ve deneyi tekrar edebilir.
- **Görünür fizik:** Kuvvet vektörleri, hareket izleri ve ölçüm sonuçları gerektiğinde görünür hâle gelir.
- **Anlamlı oyunlaştırma:** İlerleme yalnızca hızlı tamamlamaya değil, tahminlere ve kavramları anlamaya dayanır.
- **Basit ve güvenilir etkileşim:** İlk sürüm kontrolcüyle nesne tutma, bırakma ve düzenek kullanmaya odaklanır.
- **Ölçülebilir öğrenme:** Her deneyin açık bir öğrenme hedefi ve gözlemlenebilir başarı ölçütü bulunur.

---

## 2. Ürün Hedefleri ve Kapsam

### 2.1 Ana hedefler

1. Meta Quest 3 üzerinde çalışan bir VR laboratuvarı oluşturmak.
2. Aynı temel etkileşim ve ölçüm altyapısını paylaşan üç mekanik deneyi tamamlamak.
3. Deney öncesi tahmin, deney uygulama, sonuçları gözlemleme ve açıklama döngüsünü uygulamak.
4. Fizik sonuçlarını teorik beklentilerle karşılaştırarak simülasyonun tutarlılığını doğrulamak.
5. Projeyi GitHub README'si, oynanış videosu ve teknik açıklamalarla portfolyoda sunmak.

### 2.2 MVP kapsamına dahil

- Tek bir laboratuvar sahnesi
- VR kontrolcüleriyle nesne tutma ve bırakma
- Deney seçimi, başlatma, durdurma ve sıfırlama
- Kütle, kuvvet, yüzey veya rampa açısı gibi değiştirilebilir parametreler
- Hız, ivme, süre ve mesafe ölçümleri
- Tahmin soruları ve deney sonrası açıklamalar
- Basit sonuç paneli ve karşılaştırma grafiği
- Üç tamamlanmış deney
- Görev tamamlanma durumu
- Quest 3 üzerinde temel performans ve kullanılabilirlik testleri

### 2.3 MVP kapsamı dışında

- Çok oyunculu mod
- Çevrim içi hesaplar ve bulut kayıt sistemi
- Öğretmen yönetim paneli
- Tüm lise fizik müfredatı
- Karmaşık hikâye veya karakter ilerlemesi
- El takibi ve gelişmiş haptik sistemler
- Gelişmiş laboratuvar özelleştirmesi
- Ayrıntılı analitik ve kapsamlı öğrenme değerlendirme platformu

---

## 3. Hedef Kitle

### 3.1 Birincil hedef kitle

- 9–10. sınıf lise öğrencileri
- Temel mekanik kavramlarını öğrenen veya pekiştiren kullanıcılar
- VR deneyimi olan ya da olmayan öğrenciler

### 3.2 Kullanıcı ihtiyaçları

- Soyut fizik ilişkilerini gözle görülür hâle getirmek
- Parametreleri güvenli ve kolay biçimde değiştirerek deney yapabilmek
- Yanlış tahminlerde bulunup sonuçlarını görebilmek
- Sonuçları ölçerek karşılaştırabilmek
- Uzun açıklamalar okumadan, uygulamalı görevlerle ilerlemek

### 3.3 Kullanılabilirlik ilkeleri

- Talimatlar kısa ve anlaşılır olmalı.
- Önemli etkileşimler görsel veya işitsel geri bildirim vermeli.
- Oyuncu deneyi kolayca sıfırlayabilmeli.
- Başarı yalnızca deneyi hızlı bitirmeye bağlanmamalı.
- Hareket kaynaklı rahatsızlığı artırabilecek zorunlu kamera hareketlerinden kaçınılmalı.

---

## 4. Öğrenme Tasarımı

Her deney aşağıdaki döngüyü izler:

1. **Tahmin et:** Oyuncuya bir soru veya problem sunulur.
2. **Düzeneği kur:** Oyuncu ilgili nesneleri seçer veya parametreleri ayarlar.
3. **Deneyi uygula:** Oyuncu kuvvet uygular, cismi serbest bırakır veya düzeneği çalıştırır.
4. **Sonucu gözlemle:** Hareket ve ölçümler gösterilir.
5. **Karşılaştır:** Oyuncu tahminini sonuçla karşılaştırır.
6. **Açıklamayı incele:** Sistem ilgili fizik ilkesini açıklar.
7. **Tekrar dene:** Oyuncu parametreleri değiştirip deneyi tekrarlayabilir.

Yanlış tahmin, deneyi durduran bir ceza olmamalıdır. Bunun yerine sistem, tahmin ile sonuç arasındaki farkı anlamaya yardımcı olmalıdır.

---

## 5. Deney Tasarımları

### 5.1 Deney 1 — Kütle ve İvme

**Öncelik:** 1  
**Ana kavram:** Newton'un ikinci yasası  
**Temel ilişki:** `a = F_net / m`

#### Öğrenme hedefleri

Oyuncu:
- Net kuvvet sabitken farklı kütlelerin ivmelerini karşılaştırır.
- Kütle arttıkça ivmenin azaldığını gözlemler.
- Deney sonucunu net kuvvet, kütle ve ivme ilişkisiyle açıklar.

#### Düzenek

- Bir veya daha fazla fizik arabası
- Değiştirilebilir kütle blokları
- Düz bir ray veya test yüzeyi
- Kuvvet uygulama aracı veya kontrollü kuvvet mekanizması
- Ölçüm paneli
- İsteğe bağlı kuvvet vektörü ve hareket izi

#### Oyuncu akışı

1. Deney sorusu gösterilir: “Aynı net kuvvet uygulanırsa daha ağır araba nasıl hareket eder?”
2. Oyuncu tahminini seçer.
3. Arabaya bir kütle değeri atar.
4. Sabit net kuvveti uygular veya deneyi başlatır.
5. Arabanın hareketini ve ivme değerini gözlemler.
6. Farklı kütleyle deneyi tekrarlar.
7. İki sonucun karşılaştırmasını ve kısa açıklamayı görür.

#### Başarı ölçütü

Oyuncu, aynı net kuvvet altında kütle arttığında ivmenin azalacağını doğru şekilde ifade edebilir.

#### Fizik doğrulama senaryosu

Sürtünmenin ihmal edildiği kontrollü testte:
- `m = 2 kg`, `F_net = 4 N` için `a = 2 m/s²`
- `m = 4 kg`, `F_net = 4 N` için `a = 1 m/s²`

Bu değerler simülasyonun temel doğrulama testleri için kullanılabilir.

#### Sınırlar ve dikkat noktaları

- Deneyin ilk sürümünde sürtünme etkisi ihmal edilmeli veya açıkça sabit tutulmalıdır.
- Oyuncunun uyguladığı kuvvetin değişmesi, kütle etkisini karıştırmamalıdır.
- Kuvvet uygulama yöntemi deney boyunca tekrarlanabilir sonuç vermelidir.

### 5.2 Deney 2 — Sürtünme Kuvveti

**Öncelik:** 2  
**Ana kavram:** Yüzey koşulları ve hareketin yavaşlaması

#### Öğrenme hedefleri

Oyuncu:
- Aynı cismi farklı yüzeylerde test eder.
- Yüzey koşullarının hareket ve yavaşlama üzerindeki etkisini gözlemler.
- Sürtünmenin hareket üzerindeki etkisini basit bir açıklamayla ifade eder.

#### Düzenek

- Fizik arabası veya kaydırılabilir cisim
- Birbirinin yerine takılabilen yüzey parçaları
- Örneğin pürüzsüz, ahşap benzeri ve kauçuk benzeri yüzeyler
- Ölçüm paneli
- Başlangıç çizgisi ve mesafe işaretleri

#### Oyuncu akışı

1. Oyuncu hangi yüzeyde cismin daha uzun süre hareket edeceğini tahmin eder.
2. İlk yüzeyi seçer.
3. Cisme aynı başlangıç koşullarında hareket verir.
4. Hareket süresini, mesafeyi veya yavaşlamayı gözlemler.
5. İkinci yüzeyde aynı deneyi tekrarlar.
6. Sonuçları karşılaştırır ve açıklamayı inceler.

#### Başarı ölçütü

Oyuncu, farklı yüzey koşullarının sürtünmeyi ve cismin hareketini nasıl etkilediğini gözlemlere dayanarak açıklayabilir.

#### Teknik ve fiziksel notlar

- Yüzeylerin fiziksel özellikleri açıkça tanımlanmalıdır.
- Başlangıç hızı ve diğer koşullar karşılaştırma boyunca sabit tutulmalıdır.
- Unity fizik malzemelerinin davranışı test edilmeli; kullanılan parametrelerin gerçek fizik katsayılarıyla birebir aynı olduğu varsayılmamalıdır.
- Öğretici açıklamalarda yalnızca yüzeyin görünüşünden kesin sürtünme sonucu çıkarılmamalı; deney koşulları açıkça belirtilmelidir.

### 5.3 Deney 3 — Eğik Düzlem ve Hareket

**Öncelik:** 3  
**Ana kavram:** Eğim boyunca yer çekiminin etkisi

#### Öğrenme hedefleri

Oyuncu:
- Rampa açısını değiştirir.
- Aynı cismi farklı eğimlerde serbest bırakır.
- Hareketin nasıl değiştiğini gözlemler.
- Yer çekiminin eğim boyunca etkisini temel düzeyde açıklar.

#### Düzenek

- Ayarlanabilir eğik düzlem
- Fizik arabası
- Açı göstergesi
- Başlangıç ve bitiş işaretleri
- Ölçüm paneli

#### Oyuncu akışı

1. Oyuncu rampanın açısı değiştiğinde hareketin nasıl etkileneceğini tahmin eder.
2. Bir açı seçer.
3. Arabayı başlangıç noktasına yerleştirir ve serbest bırakır.
4. Hareket süresini ve ivmeyi gözlemler.
5. Rampanın açısını değiştirerek deneyi tekrarlar.
6. Sonuçları karşılaştırır ve açıklamayı inceler.

#### Başarı ölçütü

Oyuncu, sürtünme ve diğer koşullar kontrol edildiğinde eğim arttıkça eğim doğrultusundaki yer çekimi bileşeninin arttığını açıklayabilir.

#### Teknik ve fiziksel notlar

- Rampa açısı belirli ve ölçülebilir aralıkta ayarlanmalıdır.
- Araba başlangıçta aynı konumda ve başlangıç hızı sıfır olacak şekilde bırakılmalıdır.
- Sürtünme etkisi kontrol edilmeli veya öğretici açıklamada belirtilmelidir.
- Oyuncunun rampayı fiziksel olarak hareket ettirmesi zor veya kararsızsa, açı ayarı için tutma noktası ya da arayüz kontrolü kullanılabilir.

---

## 6. Temel Oynanış Sistemleri

### 6.1 VR etkileşimleri

- Nesne tutma ve bırakma
- Nesneyi belirlenmiş alanlara yerleştirme
- Düğme veya kontrol paneli kullanma
- Deneyi başlatma, durdurma ve sıfırlama
- Parametreleri kontrolcüyle veya VR arayüzüyle değiştirme

### 6.2 Deney durumları

Her deney şu durumları desteklemelidir:

1. `Introduction` — Deneyin amacı ve talimatlar
2. `Prediction` — Tahmin sorusu
3. `Setup` — Düzeneği hazırlama
4. `Ready` — Başlatmaya hazır olma
5. `Running` — Deneyin çalışması
6. `Results` — Sonuçların gösterilmesi
7. `Explanation` — Fiziksel açıklama
8. `Completed` — Görevin tamamlanması

Oyuncu deneyi istediği zaman sıfırlayabilmelidir. Sıfırlama; nesnelerin konumlarını, hızlarını, açısal hızlarını, parametrelerini, ölçüm kayıtlarını ve deney durumunu tutarlı biçimde başlangıca döndürmelidir.

### 6.3 Ölçüm sistemi

İlk sürümde ihtiyaç duyulan ölçümler:
- Geçen süre
- Başlangıç ve bitiş konumu
- Alınan mesafe
- Hız
- İvme
- Deney parametreleri

Ölçümler fiziksel simülasyondan türetilmeli; yalnızca görsel animasyon hızına dayanarak hesaplanmamalıdır. İvme hesaplanırken gürültülü türevler veya kareler arası farklar için uygun örnekleme ve yumuşatma yaklaşımı değerlendirilmelidir.

### 6.4 Geri bildirim

Geri bildirim şu unsurları içerebilir:
- Tahminin sonuçla uyuşup uyuşmadığı
- Deney ölçümlerinin özeti
- Önceki denemeyle karşılaştırma
- Kısa kavramsal açıklama
- Bir sonraki deneme için öneri

İlk sürümde karmaşık puanlama yerine doğru deneyi tamamlama, tahmini sonuçla karşılaştırma ve açıklamayı inceleme gibi basit hedefler kullanılmalıdır.

---

## 7. Teknik Mimari

### 7.1 Genel yapı

1. **VR etkileşim katmanı:** Kontrolcü girdileri, nesne tutma ve arayüz etkileşimleri.
2. **Deney yönetimi:** Deney durumu, parametreler, başlatma ve sıfırlama.
3. **Fizik sistemi:** Rigidbody, kuvvetler, sürtünme ve eğim.
4. **Ölçüm sistemi:** Zaman, mesafe, hız ve ivme kayıtları.
5. **Öğrenme sistemi:** Tahmin, sonuç, açıklama ve görev ilerlemesi.

### 7.2 Önerilen script'ler

| Script                      | Sorumluluk                                                                    |
| --------------------------- | ----------------------------------------------------------------------------- |
| `ExperimentManager.cs`      | Aktif deneyi ve deney durumunu yönetir.                                       |
| `ExperimentConfig.cs`       | Deney parametrelerini tanımlar; gerekirse ScriptableObject olarak kullanılır. |
| `CartPhysics.cs`            | Arabanın kütlesini ve fiziksel davranışını yönetir.                           |
| `ForceApplicator.cs`        | Kontrollü kuvvet uygular.                                                     |
| `SurfaceFriction.cs`        | Yüzey özelliklerini ve sürtünme parametrelerini yönetir.                      |
| `MeasurementSystem.cs`      | Hareket ölçümlerini toplar ve hesaplar.                                       |
| `ExperimentUI.cs`           | Parametreleri, yönergeleri ve sonuçları gösterir.                             |
| `LearningFlowController.cs` | Tahmin, deney, sonuç ve açıklama aşamalarını yönetir.                         |

Bu ayrım hedef mimaridir; prototipin ilk gününde bütün script'leri oluşturmak gerekli değildir. Önce çalışan en küçük deney yapılmalı, ardından sorumluluklar gerektiği ölçüde ayrılmalıdır.

### 7.3 Önerilen klasör yapısı

```text
Assets/
├── _Project/
│   ├── Scenes/
│   │   ├── Bootstrap.unity
│   │   └── PhysicsLab.unity
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Physics/
│   │   ├── Experiments/
│   │   ├── Measurement/
│   │   └── UI/
│   ├── Prefabs/
│   │   ├── Carts/
│   │   ├── Ramps/
│   │   ├── Surfaces/
│   │   └── Interactables/
│   ├── Materials/
│   ├── Models/
│   ├── Audio/
│   └── ScriptableObjects/
└── ThirdParty/
    └── VRInteractionFramework/
```

Klasör ve script isimleri geliştirme sırasında değişebilir. Üçüncü taraf asset'ler mümkün olduğunca proje kodundan ayrı tutulmalıdır.

### 7.4 Fizik uygulama kuralları

- Kuvvet tabanlı fizik işlemleri Unity fizik zamanlamasıyla uyumlu yürütülmelidir.
- Sürekli kuvvet ve darbe uygulaması birbirinden ayrılmalıdır.
- Deneyler tekrarlanabilir başlangıç koşulları sağlamalıdır.
- Fizik parametreleri değiştirilirken ölçüm ve görev durumları tutarlı kalmalıdır.
- Unity fizik motorunun davranışı teorik değerlerle karşılaştırılarak doğrulanmalıdır.
- Quest üzerinde performans testleri yapılmalıdır; Quest Link performansı bağımsız cihaz performansının garantisi değildir.

---

## 8. Kullanıcı Arayüzü ve Geri Bildirim

### 8.1 Laboratuvar içi arayüz

İlk sürüm için arayüzde şunlar bulunmalıdır:
- Aktif deneyin adı
- Kısa görev açıklaması
- Deney parametreleri
- Başlat / durdur / sıfırla kontrolleri
- Ölçüm sonuçları
- Tahmin ve sonuç karşılaştırması

### 8.2 Görsel gösterimler

- Kuvvet vektörleri
- Hareket izi
- Başlangıç ve bitiş çizgileri
- Basit çizgi veya sütun grafikleri
- Parametre değerleri ve ölçüm birimleri

Bu gösterimler öğrenme hedefini desteklemelidir. Aynı anda çok fazla gösterge kullanmak yerine oyuncuya ihtiyaç duyduğu bilgiyi aşamalı olarak sunmak tercih edilir.

---

## 9. Görsel ve İşitsel Yön

### 9.1 Görsel yaklaşım

- Temiz, anlaşılır ve çağdaş bir laboratuvar
- Öğrenme nesnelerini öne çıkaran sade çevre
- Kolay ayırt edilen etkileşimli nesneler
- Okunabilir yazı ve ölçüm panelleri
- Fizik vektörleri ve hareket izleri için tutarlı görsel dil

### 9.2 MVP için görsel öncelikler

1. Nesnelerin ve etkileşim noktalarının okunabilirliği
2. Ölçüm panelinin rahat okunması
3. Deney düzeneklerinin işlevsel olması
4. Tutarlı malzeme ve aydınlatma
5. Dekoratif ayrıntılar

### 9.3 Ses

İlk sürümde ses zorunlu değildir. İsteğe bağlı olarak:
- Nesne yerleştirme sesi
- Deney başlatma/durdurma geri bildirimi
- Görev tamamlanma sesi
- Kısa yönerge sesleri

Sesler rahatsız edici olmamalı ve önemli bilgileri yalnızca ses üzerinden vermemelidir.

---

## 10. Teknik ve Kullanılabilirlik Gereksinimleri

### 10.1 Platform

- Hedef cihaz: Meta Quest 3
- Geliştirme ve hata ayıklama: Quest Link
- Oyun motoru: Unity
- Etkileşim altyapısı: Mevcut VR Interaction Framework
- Render pipeline: Mevcut proje yapılandırması doğrulandıktan sonra URP değerlendirilebilir

### 10.2 Performans

- Hedef cihazda kararlı kare hızı amaçlanmalıdır.
- Gereksiz gerçek zamanlı ışıklar, ağır post-processing ve yüksek maliyetli fizik etkileşimleri sınırlandırılmalıdır.
- Performans yalnızca PC üzerinde değil, hedef Quest cihazında ölçülmelidir.
- Gecikme, kontrolcü etkileşimleri ve okunabilirlik düzenli olarak test edilmelidir.

### 10.3 Konfor ve erişilebilirlik

- Zorunlu yapay kamera hareketlerinden kaçınılmalıdır.
- Deneyler ayakta veya oturarak kullanılabilecek biçimde tasarlanmalıdır.
- Kullanıcıya nesneleri erişilebilir mesafede sun.
- Metinleri kısa, kontrastı yeterli ve VR içinde okunabilir tut.
- Deneyi durdurmak ve sıfırlamak kolay olmalıdır.

---

## 11. 10 Haftalık Geliştirme Planı

### Hafta 1 — Kapsam ve teknik doğrulama

**Görevler**
- Öğrenme kazanımlarını kesinleştir.
- Üç deneyin akışını tasarla.
- VR Interaction Framework'ün Unity sürümünü ve bağımlılıklarını doğrula.
- Quest Link üzerinde çalışan basit bir sahne aç.

**Çıktı:** Teknik olarak çalıştığı doğrulanmış temel VR sahnesi ve kısa deney tasarım dokümanı.

### Hafta 2 — VR etkileşim prototipi

**Görevler**
- Nesne tutma ve bırakma etkileşimlerini kur.
- Basit araba, ray ve kütle blokları oluştur.
- Deney sıfırlama yaklaşımını uygula.

**Çıktı:** Kontrolcüyle etkileşilebilen basit fizik düzeneği.

### Hafta 3 — Kütle ve kuvvet sistemi

**Görevler**
- Rigidbody ve kuvvet uygulamasını kur.
- Kütle ve net kuvveti değiştirilebilir yap.
- İvme ölçümünü teorik senaryolarla karşılaştır.

**Çıktı:** İlk deneyin temel fizik davranışı çalışıyor.

### Hafta 4 — Ölçüm ve sonuç gösterimi

**Görevler**
- Zaman, mesafe, hız ve ivme ölçümlerini ekle.
- Hareket izi veya kuvvet vektörleri oluştur.
- Basit sonuç paneli yap.

**Çıktı:** Ölçülebilir ve tekrarlanabilir ilk deney.

### Hafta 5 — Sürtünme deneyi

**Görevler**
- Değiştirilebilir yüzeyler oluştur.
- Yüzey parametrelerini yönet.
- Farklı yüzeylerde sonuçları test et.

**Çıktı:** İkinci deney aynı temel altyapıyı kullanıyor.

### Hafta 6 — Eğik düzlem deneyi

**Görevler**
- Rampa açısı ayarlama sistemini kur.
- Farklı açılarda hareketi test et.
- Sürtünme ve eğim etkilerini ayırt et.

**Çıktı:** Üçüncü deney çalışıyor.

### Hafta 7 — Eğitim ve görev akışı

**Görevler**
- Tahmin soruları ekle.
- Deney sonrası açıklamalar oluştur.
- Sonuç karşılaştırmasını uygula.
- Görev tamamlanma durumunu ekle.

**Çıktı:** Baştan sona oynanabilen öğrenme döngüsü.

### Hafta 8 — MVP doğrulaması

**Görevler**
- Üç deneyi Quest üzerinde test et.
- Reset, etkileşim ve ölçüm hatalarını düzelt.
- Temel fizik doğrulama senaryolarını çalıştır.
- Performans sorunlarını belirle.

**Çıktı:** Üç deneyin oynanabildiği MVP.

### Hafta 9 — Kullanılabilirlik ve görsel iyileştirme

**Görevler**
- Görsel dili tutarlı hâle getir.
- Yönergeleri ve okunabilirliği iyileştir.
- Mümkünse birkaç kullanıcıyla kullanılabilirlik testi yap.
- Test bulgularına göre düzenleme yap.

**Çıktı:** Daha anlaşılır ve sunulabilir prototip.

### Hafta 10 — Portfolyo ve sunum

**Görevler**
- Kısa oynanış videosu kaydet.
- GitHub README ve kurulum yönergelerini tamamla.
- Tasarım kararlarını ve sınırlamaları belgele.
- Demo akışını prova et.

**Çıktı:** Portfolyoda sergilenebilir proje.

### 8 haftalık kısaltılmış plan

Süre kısalırsa:
- İlk iki haftanın bazı işleri birleştirilebilir.
- Ölçüm grafikleri basitleştirilebilir.
- Görsel cilalama sonraya bırakılabilir.
- Gerekirse üçüncü deneyin kapsamı küçültülebilir.

**Fizik doğrulaması ve Quest cihazında test atlanmamalıdır.**

### 11–12. haftalar — Opsiyonel iyileştirme

- Kullanıcı testlerini genişletme
- Öğrenme öncesi/sonrası kısa değerlendirme
- Performans ve konfor iyileştirmeleri
- Ek görsel ve işitsel geri bildirim
- Sunum videosunu ve dokümantasyonu geliştirme

---

## 12. Test Planı ve Kabul Kriterleri

### 12.1 Teknik kabul kriterleri

- [ ] Quest Link üzerinden VR sahnesi açılıyor.
- [ ] Kontrolcü etkileşimleri üç deneyde de çalışıyor.
- [ ] Deney başlatma, durdurma ve sıfırlama işlevleri çalışıyor.
- [ ] Sıfırlama, fizik durumunu ve ölçüm kayıtlarını temizliyor.
- [ ] Hedef Quest cihazında kritik performans sorunu bulunmuyor.

### 12.2 Fizik doğruluğu kriterleri

- [ ] Sabit net kuvvette kütle arttıkça ivme azalıyor.
- [ ] Sürtünme parametreleri hareketi tutarlı etkiliyor.
- [ ] Eğim deneyinin sonuçları teorik beklentiyle uyumlu.
- [ ] Tekrarlanan deneylerde ölçümler tutarlı.
- [ ] Kullanılan varsayımlar ve sınırlamalar oyuncuya açıkça belirtiliyor.

### 12.3 Eğitim kriterleri

- [ ] Her deneyde açık bir öğrenme hedefi var.
- [ ] Her deneyde tahmin ve sonuç karşılaştırması bulunuyor.
- [ ] Oyuncu fiziksel sonucu açıklayan geri bildirim alıyor.
- [ ] Görevler fiziksel kavramı kullanmayı gerektiriyor.
- [ ] Oyuncu deneyi tekrar edip parametreleri değiştirebiliyor.

### 12.4 Portfolyo kriterleri

- [ ] Projenin amacı ve hedef kitlesi belgelenmiş.
- [ ] GitHub README ve kurulum adımları hazır.
- [ ] Oynanış videosu kaydedilmiş.
- [ ] Kod ve klasör yapısı düzenli.
- [ ] Teknik kararlar ve bilinen sınırlamalar açıklanmış.

---

## 13. Öğrenme Etkisinin Değerlendirilmesi

Bu prototip bir eğitim araştırması olarak kullanılacaksa veya ileride akademik çalışmaya dönüştürülecekse, küçük bir ön test ve son test tasarlanabilir.

Önerilen yöntem:
1. Öğrenciye deney öncesinde birkaç kavramsal soru sor.
2. VR deneyini tamamlamasını sağla.
3. Aynı kavramları farklı örneklerle tekrar ölç.
4. Sonuçları ve gözlenen kullanım sorunlarını karşılaştır.
5. Bulguları örneklem büyüklüğü ve test tasarımının sınırlamalarıyla birlikte raporla.

Bu ölçüm, prototipin öğrenmeye katkısı hakkında ilk fikirleri verebilir; tek başına genellenebilir bir eğitim etkinliği iddiası oluşturmaz.

---

## 14. Riskler ve Önlemler

| Risk | Etkisi | Önlem |
|---|---|---|
| VR asset'inin sürüm veya XR altyapısı uyumsuzluğu | Geliştirme gecikmesi | İlk hafta teknik doğrulama yap. |
| Fizik sonuçlarının teorik değerlerle uyuşmaması | Yanlış öğrenme | Basit test senaryoları ve kontrollü başlangıç koşulları kullan. |
| VR'da nesne tutma ile fizik motorunun çatışması | Kararsız etkileşim | Etkileşim ve Rigidbody davranışını erken test et. |
| Kapsamın büyümesi | MVP'nin bitmemesi | Üç deney ve tek laboratuvar kapsamını koru. |
| Quest performansının yetersiz olması | Rahatsızlık ve düşük kalite | Hedef cihazda erken ve düzenli test yap. |
| Ölçüm arayüzünün okunmaması | Öğrenme akışının bozulması | Kısa metin, yeterli kontrast ve basit grafikler kullan. |
| Deneyin yalnızca görsel olarak ikna edici olması | Kavramsal yanlışlık | Fizik varsayımlarını açıkça belirt ve ölçümleri doğrula. |

---

## 15. Portfolyo Sunumu

Portfolyo sayfası şu bölümleri içermelidir:

1. **Kısa tanıtım:** Projenin problemi ve hedef kitlesi.
2. **Tasarım hedefleri:** VR'ın öğrenme deneyimine ne kattığı.
3. **Deney videoları:** Üç deneyin oynanış akışı.
4. **Teknik mimari:** Etkileşim, fizik, ölçüm ve öğrenme sistemleri.
5. **Geliştirme süreci:** Greybox, prototip, test ve iyileştirmeler.
6. **Test sonuçları:** Fizik doğrulama senaryoları ve varsa kullanıcı testi bulguları.
7. **Sınırlamalar ve sonraki adımlar:** İlk sürümde nelerin kapsam dışında kaldığı.

GitHub deposunda proje açıklaması, Unity sürümü, gerekli üçüncü taraf asset'ler, kurulum adımları, kontrol şeması, bilinen sorunlar ve demo bağlantısı yer almalıdır. Üçüncü taraf asset'lerin lisans ve dağıtım koşulları kontrol edilmeden dosyaları depoya ekleme.

---

## 16. Gelecek Geliştirmeler

MVP tamamlandıktan sonra değerlendirilebilecek özellikler:

- Atış hareketi ve yörünge deneyleri
- İş ve enerji deneyleri
- Daha gelişmiş grafik ve veri dışa aktarma
- Öğrenci ilerleme kaydı
- Öğretmen için görev oluşturma aracı
- El takibi veya gelişmiş dokunsal geri bildirim
- Farklı zorluk seviyeleri
- Kullanıcı testleriyle desteklenen öğrenme tasarımı iyileştirmeleri

Bu özellikler ilk MVP'nin tamamlanma koşulu değildir.

---

## 17. İlk Geliştirme Görevi

İlk hafta laboratuvarın tüm modellerini veya arayüzünü üretmek yerine şu küçük prototiple başla:

1. Tek bir masa, bir araba, iki farklı kütle ve bir ray oluştur.
2. VR kontrolcüsüyle arabayı veya kütle bloklarını tutup bırakmayı doğrula.
3. Arabaya kontrollü net kuvvet uygula.
4. İvmeyi ölç ve teorik değerle karşılaştır.
5. Deneyi sıfırla ve aynı koşullarda tekrar çalıştır.

Bu beş adım çalıştığında VR etkileşimi ile fizik simülasyonunun birlikte çalıştığı doğrulanmış olur. Sonraki iki deney aynı altyapı üzerine inşa edilebilir.

---

## 18. Belge Durumu

Bu GDD, ilk prototip için başlangıç taslağıdır. Kesin Unity sürümü, VR Interaction Framework yapılandırması, kesin deney parametreleri ve performans hedefleri teknik doğrulama sonrasında netleştirilmelidir.

**MVP başarı tanımı:** Meta Quest 3 üzerinde çalışabilen, aynı laboratuvarda üç mekanik deney sunan, ölçüm ve geri bildirim içeren, test edilmiş ve portfolyoda sunulabilir bir VR fizik prototipi.
