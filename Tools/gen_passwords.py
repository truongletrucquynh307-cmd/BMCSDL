import base64
import os
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

# Key từ appsettings.json
key = base64.b64decode('LZ1Z8w2E8VMT6OYu3YcR6o7SkZb+Kx/ivB3pY7e1T5g=')

def encrypt(plain_text):
    nonce = os.urandom(12)  # 96-bit nonce
    aesgcm = AESGCM(key)
    ciphertext = aesgcm.encrypt(nonce, plain_text.encode('utf-8'), None)
    # cryptography library: ciphertext includes tag at the end
    # C# AesGcmHelper format: nonce(12) + tag(16) + ciphertext
    tag = ciphertext[-16:]
    ct = ciphertext[:-16]
    combined = nonce + tag + ct
    return base64.b64encode(combined).decode('utf-8')

passwords = [
    ('NV001', 'Admin123@', 'Quản lý'),
    ('NV002', 'Nhanvien123@', 'Bán hàng'),
    ('NV003', 'Nhanvien123@', 'Kho hàng'),
    ('NV004', 'Nhanvien123@', 'Kho hàng'),
    ('NV005', 'Nhanvien123@', 'Bán hàng'),
]

print('-- TAIKHOAN với password đã được mã hóa AES-256-GCM')
print('-- Plaintext passwords: Admin123@ và Nhanvien123@')
for manv, pwd, vaitro in passwords:
    encrypted = encrypt(pwd)
    print(f"INSERT INTO TAIKHOAN VALUES ('{manv}', '{encrypted}', N'{vaitro}');")
