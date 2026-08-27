# 📚 eLibrary Workspace - Kurumsal Arşiv Yönetim Sistemi

Bu proje, kurum içi doküman trafiğini dijitalleştirmek, rol bazlı yetkilendirme ile güvenli dosya saklama ve yönetim altyapısı sunmak amacıyla **SAYAZILIM** bünyesindeki 2. zorunlu stajım kapsamında uçtan uca (Full-Stack) geliştirilmiş bir B2B web uygulamasıdır.

## 🚀 Proje Özellikleri

- **👥 Rol Bazlı Yetkilendirme (RBAC):** Admin (Yönetici) ve Personel olmak üzere session tabanlı gelişmiş rol mimarisi. Yetkisiz sayfa erişimlerinin engellenmesi.
- **📁 Kapsamlı Doküman Yönetimi:** Kurum dosyalarının sunucuya güvenle yüklenmesi, kategorize edilmesi ve işlem geçmişlerinin (Log) anlık olarak tutulması.
- **🔐 Gelişmiş Şifre Politikaları (Password Policy):** Regex algoritmalarıyla zorunlu karmaşık şifre (harf + rakam + min. 8 karakter) kurgusu ve eski şifre kullanımının engellenmesi.
- **📧 Dinamik SMTP Entegrasyonu:** Şifresini unutan kullanıcılar için Guid tabanlı tek kullanımlık geçici şifre üretimi ve UTF-8 formatında güvenli e-posta gönderimi.
- **⚡ Asenkron Operasyonlar (AJAX):** Sayfa yenilenmeden çalışan dinamik bildirim paneli, son işlemler özeti ve adminlere özel anlık "Sunucu Doluluk Oranı" hesaplaması.
- **✨ Premium Kurumsal UI/UX:** Responsive (Mobil Uyumlu) tasarım, SweetAlert2 destekli hata/başarı bildirimleri ve "Floating Card" giriş ekranı mimarisi.

## 🛠️ Kullanılan Teknolojiler

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

## 📸 Ekran Görüntüleri


## ⚙️ Kurulum ve Çalıştırma

Projeyi yerel makinenizde (Localhost) çalıştırmak için aşağıdaki adımları izleyin:

1. Repoyu bilgisayarınıza klonlayın:
```bash
git clone [https://github.com/KULLANICI-ADIN/elibrary-workspace.git](https://github.com/KULLANICI-ADIN/elibrary-workspace.git)
