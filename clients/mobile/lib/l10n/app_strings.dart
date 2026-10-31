import 'package:flutter/material.dart';

/// Arabic / English UI copy. Default language is Arabic.
class AppStrings {
  AppStrings(this.locale);

  final Locale locale;

  bool get _ar => locale.languageCode != 'en';

  String get brand => _ar ? 'مودرن تك' : 'Modern Tech';
  String get brandEn => 'Modern Tech';

  String get languageToggle => _ar ? 'English' : 'العربية';

  // Guest Scan & Go
  String get guestTagline =>
      _ar ? 'امسح وتسوّق — ادفع عند الكاشير' : 'Scan & shop — pay at the cashier';
  String get storeCode => _ar ? 'رمز المتجر' : 'Store code';
  String get storeCodeHint => _ar ? 'أدخل رمز المتجر' : 'Enter store code';
  String get continueGuest => _ar ? 'متابعة كزائر' : 'Continue as Guest';
  String get staffEntry => _ar ? 'دخول الموظفين' : 'Staff sign in';
  String get needStoreCode => _ar ? 'يرجى إدخال رمز المتجر' : 'Please enter the store code';
  String get scanGo => _ar ? 'امسح وادفع' : 'Scan & Go';
  String get cartHint => _ar
      ? 'امسح المنتجات أثناء التسوق، ثم اعرض رمز التحويل عند الدفع.'
      : 'Scan products as you shop, then show the transfer QR at checkout.';
  String get barcode => _ar ? 'الباركود' : 'Barcode';
  String get addItem => _ar ? 'إضافة' : 'Add';
  String get scanProduct => _ar ? 'مسح منتج' : 'Scan product';
  String get emptyCart => _ar ? 'السلة فارغة' : 'Cart is empty';
  String get added => _ar ? 'تمت الإضافة' : 'Added';
  String get subtotal => _ar ? 'المجموع الفرعي' : 'Subtotal';
  String get tax => _ar ? 'الضريبة' : 'Tax';
  String get total => _ar ? 'الإجمالي' : 'Total';
  String get finishTransfer => _ar ? 'إنهاء وإرسال للكاشير' : 'Finish & transfer';
  String get showAtCashier => _ar ? 'اعرض للكاشير' : 'Show at cashier';
  String get handQr =>
      _ar ? 'سلّم هذا الرمز للكاشير لإتمام الدفع' : 'Hand this QR to the cashier to pay';
  String get done => _ar ? 'تم' : 'Done';
  String get cartEmptyHint =>
      _ar ? 'ابدأ بمسح منتج من الرف' : 'Start by scanning a shelf product';
  String get items => _ar ? 'العناصر' : 'Items';

  // Staff
  String get staffLogin => _ar ? 'تسجيل دخول الموظفين' : 'Staff sign in';
  String get tenantCode => _ar ? 'رمز الشركة' : 'Tenant code';
  String get email => _ar ? 'البريد الإلكتروني' : 'Email';
  String get password => _ar ? 'كلمة المرور' : 'Password';
  String get signIn => _ar ? 'تسجيل الدخول' : 'Sign in';
  String get signOut => _ar ? 'تسجيل الخروج' : 'Sign out';
  String get staffHome => _ar ? 'لوحة الموظفين' : 'Staff home';
  String get staffHomeHint => _ar
      ? 'أدوات العمل للمتجر — غير ظاهرة للزبائن.'
      : 'Store staff tools — not shown to customers.';
  String get syncNow => _ar ? 'مزامنة الآن' : 'Sync now';
  String get syncOffline =>
      _ar ? 'غير متصل — تم تخطي المزامنة' : 'Offline — sync skipped';
  String get offlineBanner => _ar
      ? 'غير متصل — العمل المحلي متاح'
      : 'You’re offline — local work still available';
  String get backOnline => _ar ? 'عدت للاتصال' : 'You’re back online';
  String get syncDisabledOffline =>
      _ar ? 'المزامنة تحتاج اتصالاً' : 'Sync requires a connection';
  String pendingOutbox(int count) => _ar
      ? 'عمليات معلّقة في الطابور: $count'
      : 'Pending offline ops: $count';
  String get signedInAs => _ar ? 'مسجّل كـ' : 'Signed in as';
  String get syncNeedRelogin => _ar
      ? 'المزامنة تحتاج جهاز مسجّل. سجّل الخروج ثم الدخول مرة أخرى.'
      : 'Sync needs a registered device. Sign out and sign in again.';
  String syncResult(int pushed, int pulled) => _ar
      ? 'تم الدفع $pushed · السحب $pulled'
      : 'Pushed $pushed, pulled $pulled';

  String get products => _ar ? 'المنتجات / الباركود' : 'Products / barcode';
  String get registerProduct =>
      _ar ? 'تسجيل منتج (مسح)' : 'Register product (scan)';
  String get inventoryOps => _ar ? 'عمليات المخزون' : 'Inventory ops';
  String get posCheckout =>
      _ar ? 'نقطة البيع واستلام الطلب' : 'POS & receive order';
  String get importOrder => _ar ? 'استيراد طلب الزبون' : 'Import customer order';
  String get orderQr => _ar ? 'رمز طلب الزبون' : 'Customer order QR';
  String get orderQrHint => _ar ? 'MTSC:… أو الرمز' : 'MTSC:… or code';
  String get warehouse => _ar ? 'المستودع' : 'Warehouse';
  String get warehouseId => _ar ? 'المستودع' : 'Warehouse';
  String get product => _ar ? 'المنتج' : 'Product';
  String get addToCart => _ar ? 'إضافة إلى السلة' : 'Add to cart';
  String get completePay =>
      _ar ? 'إتمام الدفع (قائمة الانتظار)' : 'Complete / pay (queue)';
  String get randomAudit => _ar ? 'تدقيق عشوائي' : 'Random audit';
  String get auditCancel => _ar ? 'إلغاء' : 'Cancel';
  String get scanNext => _ar ? 'مسح التالي' : 'Scan next';
  String get auditCancelled => _ar ? 'تم إلغاء التدقيق' : 'Audit cancelled';
  String get needWarehouseCart =>
      _ar ? 'المستودع والسلة مطلوبان' : 'Warehouse and cart required';
  String get completeAuditFirst =>
      _ar ? 'أكمل التدقيق العشوائي قبل الدفع' : 'Complete random audit before payment';

  // Products list
  String get searchProducts =>
      _ar ? 'بحث بالرمز أو الاسم أو الباركود' : 'Search SKU, name, or barcode';
  String get noProducts =>
      _ar ? 'لا توجد منتجات. زامن أو سجّل منتجاً.' : 'No products. Sync or register one.';
  String get offlineCacheBanner =>
      _ar ? 'عرض من الذاكرة المحلية (غير متصل)' : 'Showing local cache (offline)';
  String get edit => _ar ? 'تعديل' : 'Edit';
  String get delete => _ar ? 'حذف' : 'Delete';
  String get save => _ar ? 'حفظ' : 'Save';
  String get confirmDelete =>
      _ar ? 'إلغاء تفعيل هذا المنتج؟' : 'Deactivate this product?';
  String get productName => _ar ? 'اسم المنتج' : 'Product name';
  String get salePrice => _ar ? 'سعر البيع' : 'Sale price';
  String get cancel => _ar ? 'إلغاء' : 'Cancel';
  String get refresh => _ar ? 'تحديث' : 'Refresh';

  // Inventory
  String get inventoryTitle => _ar ? 'عمليات المخزون' : 'Inventory ops';
  String get inventoryHint => _ar
      ? 'اختر العملية والمستودع والمنتج ثم أضف الكمية إلى قائمة الانتظار.'
      : 'Choose operation, warehouse, and product, then queue the quantity.';
  String get operation => _ar ? 'العملية' : 'Operation';
  String get opReceive => _ar ? 'استلام' : 'Receive';
  String get opIssue => _ar ? 'صرف' : 'Issue';
  String get opAdjust => _ar ? 'تسوية' : 'Adjust';
  String get opTransfer => _ar ? 'تحويل' : 'Transfer';
  String get quantity => _ar ? 'الكمية' : 'Quantity';
  String get queueOffline => _ar ? 'إضافة للطابور دون اتصال' : 'Queue offline';
  String get filterProducts => _ar ? 'تصفية المنتجات' : 'Filter products';
  String get selectWarehouse => _ar ? 'اختر المستودع' : 'Select warehouse';
  String get selectProduct => _ar ? 'اختر المنتج' : 'Select product';
  String get needWarehouseProductQty => _ar
      ? 'المستودع والمنتج وكمية غير صفرية مطلوبة'
      : 'Warehouse, product, and non-zero qty required';
  String queuedOp(String op) =>
      _ar ? 'تم وضع $op في الطابور (يُرسل عند المزامنة)' : 'Queued $op (will push on sync)';
  String get cachedBalances => _ar ? 'الأرصدة المخزّنة' : 'Cached balances';
  String get noBalances =>
      _ar ? 'لا أرصدة مخزّنة — زامن أولاً.' : 'No balances cached — sync first.';
  String get close => _ar ? 'إغلاق' : 'Close';
  String get torch => _ar ? 'الإضاءة' : 'Torch';
  String get cameraDenied => _ar
      ? 'إذن الكاميرا مرفوض. فعّل الكاميرا أو أدخل الباركود يدوياً.'
      : 'Camera permission denied. Enable camera or type the barcode.';
  String get cameraUnavailable =>
      _ar ? 'الكاميرا غير متاحة. أدخل الباركود يدوياً.' : 'Camera unavailable. Type the barcode.';
  String get loadingWarehouses =>
      _ar ? 'جاري تحميل المستودعات…' : 'Loading warehouses…';
  String get noWarehouses =>
      _ar ? 'لا مستودعات. أنشئ مستودعاً من لوحة الويب.' : 'No warehouses. Create one on the web dashboard.';

  // Receipts
  String get receipt => _ar ? 'الإيصال' : 'Receipt';
  String get orderHistory => _ar ? 'سجل الطلبات' : 'Order history';
  String get noReceipts =>
      _ar ? 'لا إيصالات بعد. أكمل عملية بيع أولاً.' : 'No receipts yet. Complete a sale first.';
  String get shareReceipt => _ar ? 'مشاركة الإيصال' : 'Share receipt';
  String get customerPhone => _ar ? 'هاتف الزبون (اختياري)' : 'Customer phone (optional)';
  String get customerEmail => _ar ? 'بريد الزبون (اختياري)' : 'Customer email (optional)';
  String get orderCodeLabel => _ar ? 'رمز الطلب' : 'Order code';
  String get receiptSaved => _ar ? 'تم حفظ الإيصال' : 'Receipt saved';
  String get backToPos => _ar ? 'العودة لنقطة البيع' : 'Back to POS';

  // Auth register + biometrics
  String get emailOrPhone => _ar ? 'البريد أو رقم الهاتف' : 'Email or phone';
  String get phone => _ar ? 'رقم الهاتف' : 'Phone number';
  String get displayName => _ar ? 'الاسم المعروض' : 'Display name';
  String get confirmPassword => _ar ? 'تأكيد كلمة المرور' : 'Confirm password';
  String get createAccount => _ar ? 'إنشاء حساب موظف' : 'Create staff account';
  String get registerStaff => _ar ? 'تسجيل موظف جديد' : 'Register staff';
  String get haveAccountSignIn => _ar ? 'لديك حساب؟ سجّل الدخول' : 'Have an account? Sign in';
  String get passwordsMismatch =>
      _ar ? 'كلمتا المرور غير متطابقتين' : 'Passwords do not match';
  String get saveWithBiometrics =>
      _ar ? 'حفظ الدخول ببصمة الوجه / الإصبع؟' : 'Save sign-in with Face ID / fingerprint?';
  String get saveBiometricsYes => _ar ? 'حفظ' : 'Save';
  String get saveBiometricsNo => _ar ? 'لاحقاً' : 'Not now';
  String get signInWithBiometrics =>
      _ar ? 'دخول بالبصمة' : 'Sign in with biometrics';
  String get forgetSavedPassword =>
      _ar ? 'نسيان كلمة المرور المحفوظة' : 'Forget saved password';
  String get biometricsFailed =>
      _ar ? 'تعذّر التحقق بالبصمة' : 'Biometric unlock failed';
  String get biometricsUnavailable =>
      _ar ? 'البصمة غير متاحة على هذا الجهاز' : 'Biometrics unavailable on this device';
}
