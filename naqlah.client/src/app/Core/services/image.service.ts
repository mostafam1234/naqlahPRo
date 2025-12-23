import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

export interface ImageUploadResult {
  success: boolean;
  base64?: string;
  preview?: string;
  error?: string;
}

export interface ImageValidationOptions {
  maxSizeMB?: number;
  allowedTypes?: string[];
  showErrorAlert?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ImageService {

  private readonly defaultOptions: ImageValidationOptions = {
    maxSizeMB: 5,
    allowedTypes: ['image/png', 'image/jpg', 'image/jpeg'],
    showErrorAlert: true
  };

  /**
   * تحويل الملف المختار إلى Base64 مع validation
   * @param file الملف المختار
   * @param options خيارات التحقق من الملف
   * @returns Promise مع نتيجة التحويل
   */
  async convertFileToBase64(file: File, options?: ImageValidationOptions): Promise<ImageUploadResult> {
    const config = { ...this.defaultOptions, ...options };

    // تحقق من نوع الملف
    if (!this.isValidImageFile(file, config.allowedTypes!)) {
      const error = 'يرجى اختيار ملف صورة صالح (PNG, JPG, JPEG)';
      if (config.showErrorAlert) {
        this.showErrorAlert('خطأ في نوع الملف', error);
      }
      return { success: false, error };
    }

    // تحقق من حجم الملف
    if (file.size > config.maxSizeMB! * 1024 * 1024) {
      const error = `حجم الملف كبير جداً. يرجى اختيار ملف أصغر من ${config.maxSizeMB} ميجابايت`;
      if (config.showErrorAlert) {
        this.showErrorAlert('خطأ في حجم الملف', error);
      }
      return { success: false, error };
    }

    try {
      const result = await this.fileToBase64(file);
      return {
        success: true,
        base64: this.extractBase64(result),
        preview: result
      };
    } catch (error) {
      const errorMsg = 'حدث خطأ أثناء تحويل الصورة';
      if (config.showErrorAlert) {
        this.showErrorAlert('خطأ', errorMsg);
      }
      return { success: false, error: errorMsg };
    }
  }

  /**
   * التعامل مع event رفع الصورة
   * @param event حدث اختيار الملف
   * @param options خيارات التحقق
   * @returns Promise مع نتيجة التحويل أو null إذا لم يتم اختيار ملف
   */
  async handleImageUpload(event: Event, options?: ImageValidationOptions): Promise<ImageUploadResult | null> {
    const input = event.target as HTMLInputElement;
    
    if (input.files && input.files[0]) {
      return await this.convertFileToBase64(input.files[0], options);
    }
    
    return null;
  }

  /**
   * تحقق من صحة نوع الملف
   * @param file الملف
   * @param allowedTypes الأنواع المسموحة
   * @returns true إذا كان النوع صحيح
   */
  private isValidImageFile(file: File, allowedTypes: string[]): boolean {
    return allowedTypes.includes(file.type);
  }

  /**
   * تحويل الملف إلى Base64
   * @param file الملف
   * @returns Promise مع البيانات كـ Data URL
   */
  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        resolve(result);
      };
      reader.onerror = () => reject(new Error('فشل في قراءة الملف'));
      reader.readAsDataURL(file);
    });
  }

  /**
   * استخراج Base64 من Data URL
   * @param dataUrl البيانات كـ Data URL
   * @returns Base64 string بدون البادئة
   */
  private extractBase64(dataUrl: string): string {
    return dataUrl.split(',')[1];
  }

  /**
   * عرض رسالة خطأ
   * @param title عنوان الخطأ
   * @param text نص الخطأ
   */
  private showErrorAlert(title: string, text: string): void {
    Swal.fire({
      title,
      text,
      icon: 'error',
      confirmButtonText: 'موافق',
      confirmButtonColor: '#ef4444'
    });
  }

  /**
   * تحقق من أن الـ string هو Base64 صالح
   * @param str النص المراد التحقق منه
   * @returns true إذا كان Base64 صالح
   */
  isValidBase64(str: string): boolean {
    try {
      return btoa(atob(str)) === str;
    } catch (err) {
      return false;
    }
  }

  /**
   * تحويل Base64 إلى Data URL للعرض
   * @param base64 البيانات كـ Base64
   * @param mimeType نوع الملف (افتراضي: image/jpeg)
   * @returns Data URL للعرض في img tag
   */
  base64ToDataUrl(base64: string, mimeType: string = 'image/jpeg'): string {
    return `data:${mimeType};base64,${base64}`;
  }

  /**
   * حذف reference للصورة وتنظيف الذاكرة
   * @param imageUrl رابط الصورة المراد حذفها
   */
  clearImagePreview(imageUrl: string | null): void {
    if (imageUrl && imageUrl.startsWith('blob:')) {
      URL.revokeObjectURL(imageUrl);
    }
  }
}