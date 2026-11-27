# VR Physics Lab — Gereken 3D Model Listesi (Asset Backlog)

Bu belge, projede geçici olarak küp/silindir gibi ilkel (primitive) objelerle temsil edilen ve daha sonra Blender/Unity ile nihai 3D modellerle değiştirilecek tüm varlıkların (assets) takibini sağlar. Her aşamada güncellenir.

---

## Model Standartları (Meta Quest 3 Hedefi İçin)
* **Ölçek:** 1 Unity Birimi = 1 Metre (Gerçek dünya ölçeği).
* **Poligon Bütçesi:** Prop başına 500 – 3.000 üçgen (VR performansı için optimize, low/mid-poly).
* **Pivot Noktaları:** 
  * Masalar ve zemin nesneleri için: Alt merkez (Bottom-Center).
  * Arabalar ve hareketli parçalar için: Geometrik merkez (Center of Mass).
* **Format:** `.fbx` veya `.blend` (PBR dokular: Albedo, Metallic-Smoothness, Normal).

---

## Aşama 1: Laboratuvar Çevresi ve Temel Mobilyalar

| # | Model Adı | Açıklama ve Özellikler | Yaklaşık Boyut (G x Y x D) | Mevcut Geçici Obje (Placeholder) |
|---|---|---|---|---|
| 1 | **Laboratuvar Masası (`SM_LabTable`)** | Ayakta çalışan VR oyuncusunun bel hizasında, metal ayaklı, koyu ahşap/kompozit üst tablalı modern laboratuvar çalışma masası. | $2.2\text{m} \times 0.9\text{m} \times 0.9\text{m}$ | `LabTable` (Küp tabla + 4 küp bacak) |
| 2 | **Deney Kılavuz Rayı (`SM_PhysicsTrack`)** | Üzerinde mesafe cetveli (cm/mm işaretleri) bulunan, arabanın düz hat boyunca kaymasını sağlayan alüminyum/çelik çift ray. | $1.8\text{m} \times 0.05\text{m} \times 0.25\text{m}$ | `Track` (Küp zemin + 2 yan kılavuz küpü) |
| 3 | **Laboratuvar Odası (`SM_LabRoom`)** | Çağdaş, aydınlık, beyaz/gri fayans zeminli, sade duvarlı, pencereli fizik laboratuvarı iç mekânı. | $10\text{m} \times 3.5\text{m} \times 10\text{m}$ | `Floor` (Düz gri zemin) |

---

## Aşama 2: Dinamik Fizik ve Ölçüm Donanımları

| # | Model Adı | Açıklama ve Özellikler | Yaklaşık Boyut (G x Y x D) | Mevcut Geçici Obje (Placeholder) |
|---|---|---|---|---|
| 4 | **Fizik Arabası (`SM_PhysicsCart`)** | 4 adet düşük sürtünmeli rulmanlı tekerleğe sahip, üstünde ağırlık bloklarının tam oturabileceği yuvası olan dinamik deney arabası. | $0.20\text{m} \times 0.08\text{m} \times 0.16\text{m}$ | ✅ `Assets/_Project/Models/car.fbx` (Tamamlandı) |
| 5 | **1 kg Ağırlık Bloğu (`SM_Weight_1kg`)** | Üstünde tutma kulpu veya girintisi olan, üzerinde "1.0 kg" kabartması/yazısı bulunan metal/döküm blok. | $0.08\text{m} \times 0.08\text{m} \times 0.08\text{m}$ | ✅ `Assets/_Project/Models/Weights.fbx` (Tamamlandı) |
| 6 | **2 kg Ağırlık Bloğu (`SM_Weight_2kg`)** | 1 kg'lık blokla aynı tasarım dilinde, ancak 2 kat kütleyi hissettiren "2.0 kg" yazılı metal blok. | $0.10\text{m} \times 0.10\text{m} \times 0.10\text{m}$ | ✅ `Assets/_Project/Models/Weights.fbx` (Tamamlandı) |
| 7 | **Kuvvet Uygulayıcı / İtici Piston (`SM_ForceLauncher`)** | Rayın başında duran, yayın veya pnömatik pistonun arabaya sabit net kuvvet uygulamasını sağlayan mekanizma ve üzerinde başlatma butonu. | $0.15\text{m} \times 0.12\text{m} \times 0.20\text{m}$ | `ForceLauncher` (Küp gövde + silindir piston) |
| 8 | **Fotosel Sensör Kapıları (`SM_PhotogateGate` x2)** | Ray üzerine takılan, kızılötesi ışınla arabanın geçişini algılayan U biçimli başlangıç ve bitiş sensör ayakları (LED durum ışıklı). | $0.05\text{m} \times 0.18\text{m} \times 0.28\text{m}$ | `StartGate`, `EndGate` (Küp direkler + sensör çizgisi) |
| 9 | **Dijital Ölçüm Monitörü (`SM_MeasurementMonitor`)** | Masaya monteli veya ayaklı, oyuncunun rahatça okuyabileceği açıda duran, süre, hız ve ivmeyi gösteren LED/LCD dijital gösterge kutusu. | $0.70\text{m} \times 0.38\text{m} \times 0.08\text{m}$ | `MeasurementDisplay` (Küp kasa + LCD WorldSpace Canvas) |
| 10 | **Endüstriyel Sıfırlama Butonu (`SM_ResetButton_Yellow`)** | Masaya monteli, sarı renkli, fosforlu/emissive aydınlatmalı, avuç içi veya parmakla kolayca basılabilen yaylı acil durum/sıfırlama mantar butonu. | $0.14\text{m} \times 0.05\text{m} \times 0.14\text{m}$ | `ResetButton` (Silindir kaide + sarı silindir buton başlığı) |
| 11 | **Fırlatma / Başlatma Butonu (`SM_LaunchButton_Red`)** | Kuvvet uygulayıcının üzerinde yer alan, basıldığında yaylı pistonu tetikleyen kırmızı renkli endüstriyel buton. | $0.12\text{m} \times 0.04\text{m} \times 0.12\text{m}$ | `LaunchButton` (Silindir kaide + kırmızı silindir buton başlığı) |

---

## Aşama 3: İstasyon 2 (Sürtünme Kuvveti Deneyi Donanımları)

| # | Model Adı | Açıklama ve Özellikler | Yaklaşık Boyut (G x Y x D) | Mevcut Geçici Obje (Placeholder) |
|---|---|---|---|---|
| 12 | **Sürtünme Kızağı / Arabası (`SM_FrictionCart`)** | Altında değiştirilebilir veya sürtünme test tabanı bulunan, $m = 1.0\text{ kg}$ sabit kütleli, ray üzerinde durma mesafesini gösteren ibreli kızak. | $0.18\text{m} \times 0.06\text{m} \times 0.16\text{m}$ | ✅ `Assets/_Project/Models/car.fbx` (Tamamlandı) |
| 13 | **Buz / Teflon Yüzey Plakası (`SM_SurfacePlate_Ice`)** | Düşük sürtünme katsayısına ($\mu = 0.05$) sahip, buz mavisi/teflon dokulu, VR tutma kulplu tak-çıkar yüzey kaseti. | $0.24\text{m} \times 0.03\text{m} \times 0.16\text{m}$ | `SurfacePlate_Ice` (Açık mavi küp plaka + kulp) |
| 14 | **Ahşap / Zımpara Yüzey Plakası (`SM_SurfacePlate_Wood`)** | Orta sürtünme katsayısına ($\mu = 0.20$) sahip, meşe ahşap/zımpara dokulu, VR tutma kulplu tak-çıkar yüzey kaseti. | $0.24\text{m} \times 0.03\text{m} \times 0.16\text{m}$ | `SurfacePlate_Wood` (Ahşap kahvesi küp plaka + kulp) |
| 15 | **Kauçuk / Lastik Yüzey Plakası (`SM_SurfacePlate_Rubber`)** | Yüksek sürtünme katsayısına ($\mu = 0.55$) sahip, mat siyah desenli kauçuk dokulu, VR tutma kulplu tak-çıkar yüzey kaseti. | $0.24\text{m} \times 0.03\text{m} \times 0.16\text{m}$ | `SurfacePlate_Rubber` (Koyu antrasit küp plaka + kulp) |
| 16 | **Yüzey Plakası Standı / Yuvalığı (`SM_SurfacePlate_Tray`)** | Masanın önünde yer alan, 3 farklı yüzey kasetini dik/eğik tutan etiketli masaüstü kaset yuvası. | $0.80\text{m} \times 0.03\text{m} \times 0.22\text{m}$ | `SurfacePlate_Tray` (Kaset standı + 3D etiket) |
| 17 | **Sabit İtici Mekanizma (`SM_FrictionImpulseLauncher`)** | Her denemede arabaya tam olarak $v_0 = 1.2\text{ m/s}$ başlangıç hızı kazandıran yaylı/pnömatik sabit hız fırlatıcısı. | $0.12\text{m} \times 0.08\text{m} \times 0.16\text{m}$ | `FrictionLauncher` (Küp gövde + piston mili) |
| 18 | **Sürtünme Telemetri Ekranı (`SM_FrictionDisplayMonitor`)** | Durma mesafesini, süresini, ivmeyi ve deneysel/teorik $\mu$ katsayısını bağıl hata yüzdesiyle canlı hesaplayıp gösteren monitör. | $0.70\text{m} \times 0.38\text{m} \times 0.08\text{m}$ | `MeasurementDisplay_Friction` (Kasa + WorldSpace Canvas) |

---

## Aşama 4: İstasyon 3 (Eğik Düzlem Deneyi Donanımları)

| # | Model Adı | Açıklama ve Özellikler | Yaklaşık Boyut (G x Y x D) | Mevcut Geçici Obje (Placeholder) |
|---|---|---|---|---|
| 19 | **Eğik Düzlem Arabası (`SM_InclinedCart`)** | Sürtünmesiz kabul edilen rampa boyunca serbest bırakıldığında $a = g \cdot \sin\theta$ ivmesiyle hızlanan deney arabası. | $0.20\text{m} \times 0.08\text{m} \times 0.16\text{m}$ | ✅ `Assets/_Project/Models/car.fbx` (Tamamlandı) |
| 20 | **Ayarlanabilir Eğik Düzlem Rampası (`SM_AdjustableRamp`)** | Açısı $10^\circ - 45^\circ$ arasında $5^\circ$ adımlarla ayarlanabilen, mafsallı tabanlı, teleskopik destek ayaklı ve yan kılavuz raylı rampa mekanizması. | $1.50\text{m} \times 0.06\text{m} \times 0.26\text{m}$ | `InclinedRamp_Assembly` (Küp ray/korkuluk + silindir mafsal & teleskopik ayak) |
| 21 | **Açı Kontrol Butonları (`SM_AngleButton_Blue` x2)** | Rampanın eğim açısını $-5^\circ$ azaltan ve $+5^\circ$ artıran mavi renkli endüstriyel buton çifti. | $0.14\text{m} \times 0.05\text{m} \times 0.14\text{m}$ | `AngleDownButton`, `AngleUpButton` (Mavi silindir buton başlığı) |
| 22 | **Eğik Düzlem Telemetri Monitörü (`SM_InclinedDisplayMonitor`)** | İniş süresi, eğim mesafesi, bitiş hızı, deneysel/teorik ivme ($g \cdot \sin\theta$), paralel kuvvet ($mg\sin\theta$) ve bağıl hatayı canlı gösteren yüksek ayaklı monitör. | $0.70\text{m} \times 0.38\text{m} \times 0.08\text{m}$ | `MeasurementDisplay_Inclined` (Kasa + uzun ayak + WorldSpace Canvas) |

---

## Gelecek Aşamalar İçin Planlanan Modeller (Ön Liste)
* **Kuvvet & İvme Vektör Okları:** Arabaların ve kızakların üzerinde anlık kuvvet ($F_{\text{net}}, f_s, mg\sin\theta, N$) ve ivmeyi gösteren dinamik 3D ok modelleri (`SM_ForceVectorArrow`).
