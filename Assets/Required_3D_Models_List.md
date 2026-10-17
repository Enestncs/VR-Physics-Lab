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
| 4 | **Fizik Arabası (`SM_PhysicsCart`)** | 4 adet düşük sürtünmeli rulmanlı tekerleğe sahip, üstünde ağırlık bloklarının tam oturabileceği yuvası olan dinamik deney arabası. | $0.20\text{m} \times 0.08\text{m} \times 0.16\text{m}$ | `PhysicsCart` (Küp gövde + silindir tekerlekler) |
| 5 | **1 kg Ağırlık Bloğu (`SM_Weight_1kg`)** | Üstünde tutma kulpu veya girintisi olan, üzerinde "1.0 kg" kabartması/yazısı bulunan metal/döküm blok. | $0.08\text{m} \times 0.08\text{m} \times 0.08\text{m}$ | `Weight_1kg` (Turuncu küp) |
| 6 | **2 kg Ağırlık Bloğu (`SM_Weight_2kg`)** | 1 kg'lık blokla aynı tasarım dilinde, ancak 2 kat kütleyi hissettiren "2.0 kg" yazılı metal blok. | $0.10\text{m} \times 0.10\text{m} \times 0.10\text{m}$ | `Weight_2kg` (Turuncu büyük küp) |
| 7 | **Kuvvet Uygulayıcı / İtici Piston (`SM_ForceLauncher`)** | Rayın başında duran, yayın veya pnömatik pistonun arabaya sabit net kuvvet uygulamasını sağlayan mekanizma ve üzerinde başlatma butonu. | $0.15\text{m} \times 0.12\text{m} \times 0.20\text{m}$ | `ForceLauncher` (Küp gövde + silindir piston) |
| 8 | **Fotosel Sensör Kapıları (`SM_PhotogateGate` x2)** | Ray üzerine takılan, kızılötesi ışınla arabanın geçişini algılayan U biçimli başlangıç ve bitiş sensör ayakları (LED durum ışıklı). | $0.05\text{m} \times 0.18\text{m} \times 0.28\text{m}$ | `StartGate`, `EndGate` (Küp direkler + sensör çizgisi) |
| 9 | **Dijital Ölçüm Monitörü (`SM_MeasurementMonitor`)** | Masaya monteli veya ayaklı, oyuncunun rahatça okuyabileceği açıda duran, süre, hız ve ivmeyi gösteren LED/LCD dijital gösterge kutusu. | $0.70\text{m} \times 0.38\text{m} \times 0.08\text{m}$ | `MeasurementDisplay` (Küp kasa + LCD WorldSpace Canvas) |
| 10 | **Endüstriyel Sıfırlama Butonu (`SM_ResetButton_Yellow`)** | Masaya monteli, sarı renkli, fosforlu/emissive aydınlatmalı, avuç içi veya parmakla kolayca basılabilen yaylı acil durum/sıfırlama mantar butonu. | $0.14\text{m} \times 0.05\text{m} \times 0.14\text{m}$ | `ResetButton` (Silindir kaide + sarı silindir buton başlığı) |
| 11 | **Fırlatma / Başlatma Butonu (`SM_LaunchButton_Red`)** | Kuvvet uygulayıcının üzerinde yer alan, basıldığında yaylı pistonu tetikleyen kırmızı renkli endüstriyel buton. | $0.12\text{m} \times 0.04\text{m} \times 0.12\text{m}$ | `LaunchButton` (Silindir kaide + kırmızı silindir buton başlığı) |

---

## Gelecek Aşamalar İçin Planlanan Modeller (Ön Liste)
* **Aşama 3 (Kuvvet Vektör Oku):** Arabanın üzerinde anlık ivme ve net kuvvet doğrultusunu gösteren 3D ok modeli (`SM_ForceVectorArrow`).
* **Aşama 5 (Sürtünme Yüzey Plakaları):** Rayın üzerine takılıp çıkarılabilen Buz/Teflon, Zımpara/Ahşap ve Kauçuk/Lastik dokulu yüzey plakaları (`SM_SurfacePlate_Smooth`, `SM_SurfacePlate_Rough`, `SM_SurfacePlate_Rubber`).
* **Aşama 5 (Açısı Ayarlanabilir Eğik Düzlem / Rampa):** Mafsallı, derece göstergeli rampa ayağı (`SM_AdjustableRamp`).
