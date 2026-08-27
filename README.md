# 📚 eLibrary Workspace - Kurumsal Arşiv Yönetim Sistemi

Bu proje, kurum içi doküman trafiğini dijitalleştirmek, rol bazlı yetkilendirme ile güvenli dosya saklama ve yönetim altyapısı sunmak amacıyla **SAYAZILIM** bünyesindeki 2. zorunlu stajım kapsamında uçtan uca (Full-Stack) geliştirilmiş bir B2B web uygulamasıdır.

##  Proje Özellikleri

- ** Rol Bazlı Yetkilendirme (RBAC):** Admin (Yönetici) ve Personel olmak üzere session tabanlı gelişmiş rol mimarisi. Yetkisiz sayfa erişimlerinin engellenmesi.
- ** Kapsamlı Doküman Yönetimi:** Kurum dosyalarının sunucuya güvenle yüklenmesi, kategorize edilmesi ve işlem geçmişlerinin (Log) anlık olarak tutulması.
- ** Gelişmiş Şifre Politikaları (Password Policy):** Regex algoritmalarıyla zorunlu karmaşık şifre (harf + rakam + min. 8 karakter) kurgusu ve eski şifre kullanımının engellenmesi.
- ** Dinamik SMTP Entegrasyonu:** Şifresini unutan kullanıcılar için Guid tabanlı tek kullanımlık geçici şifre üretimi ve UTF-8 formatında güvenli e-posta gönderimi.
- ** Asenkron Operasyonlar (AJAX):** Sayfa yenilenmeden çalışan dinamik bildirim paneli, son işlemler özeti ve adminlere özel anlık "Sunucu Doluluk Oranı" hesaplaması.
- ** Premium Kurumsal UI/UX:** Responsive (Mobil Uyumlu) tasarım, SweetAlert2 destekli hata/başarı bildirimleri ve "Floating Card" giriş ekranı mimarisi.

##  Kullanılan Teknolojiler

**Backend:**
* C#
* ASP.NET MVC
* Entity Framework (Code/Database First)

**Veritabanı:**
* Microsoft SQL Server
* LINQ

**Frontend:**
* HTML5 / CSS3
* Bootstrap 4.5
* JavaScript / jQuery & AJAX
* SweetAlert2 (Dinamik UI Bildirimleri)
* FontAwesome (İkonlar)

##  Ekran Görüntüleri
<img width="1187" height="875" alt="Ekran görüntüsü 2026-08-27 230553" src="https://github.com/user-attachments/assets/e917f467-6e6c-48be-af0f-5ff9ca970c6c" />
<img width="1900" height="911" alt="Ekran görüntüsü 2026-08-27 231700" src="https://github.com/user-attachments/assets/96a38945-c058-4c4d-af7c-475012abcc4b" />
<img width="1892" height="907" alt="Ekran görüntüsü 2026-08-27 231711" src="https://github.com/user-attachments/assets/51fe1040-9c6a-4246-bb7b-c13d56b1f9c9" />
<img width="1900" height="907" alt="Ekran görüntüsü 2026-08-27 231729" src="https://github.com/user-attachments/assets/47a8c100-770b-45ee-9df0-1415e122af34" />
<img width="1897" height="913" alt="Ekran görüntüsü 2026-08-27 231739" src="https://github.com/user-attachments/assets/3be0c422-f5f5-43fc-b97b-3f2c13505b37" />

<img width="1912" height="906" alt="Ekran görüntüsü 2026-08-27 231816" src="https://github.com/user-attachments/assets/e91e3a4e-4cc0-4820-9782-08c692285d6e" />
<img width="1895" height="906" alt="Ekran görüntüsü 2026-08-27 232134" src="https://github.com/user-attachments/assets/41446cc3-d800-45a8-8017-c70082821c2f" />


<img width="1907" height="912" alt="Ekran görüntüsü 2026-08-27 232810" src="https://github.com/user-attachments/assets/25137d6c-03fe-4c32-9e5f-aa5dbee2cf52" />

##  Kurulum ve Çalıştırma Rehberi

Projeyi kendi bilgisayarınızda (Localhost) sorunsuz bir şekilde ayağa kaldırmak için aşağıdaki adımları sırasıyla uygulayabilirsiniz:

**1. Projeyi Bilgisayarınıza İndirin:**
Terminal veya Komut İstemi'ni (CMD) açarak projeyi bilgisayarınıza klonlayın:
```bash
git clone [https://github.com/ozlemsenn/elibrary-workspace.git](https://github.com/ozlemsenn/elibrary-workspace.git)
