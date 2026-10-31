import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../auth/auth_repository.dart';
import '../auth/biometric_credentials.dart';
import '../auth/session_controller.dart';
import '../l10n/locale_controller.dart';
import 'home_screen.dart';

class RegisterStaffScreen extends StatefulWidget {
  const RegisterStaffScreen({super.key});

  @override
  State<RegisterStaffScreen> createState() => _RegisterStaffScreenState();
}

class _RegisterStaffScreenState extends State<RegisterStaffScreen> {
  final _tenantCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();
  final _nameCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  final _confirmCtrl = TextEditingController();
  final _biometrics = BiometricCredentials();
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      final tenant = await context.read<AuthRepository>().rememberedTenantCode;
      if (!mounted || tenant == null) return;
      _tenantCtrl.text = tenant;
    });
  }

  @override
  void dispose() {
    _tenantCtrl.dispose();
    _emailCtrl.dispose();
    _phoneCtrl.dispose();
    _nameCtrl.dispose();
    _passwordCtrl.dispose();
    _confirmCtrl.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final s = context.read<LocaleController>().s;
    if (_passwordCtrl.text != _confirmCtrl.text) {
      setState(() => _error = s.passwordsMismatch);
      return;
    }
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final auth = context.read<AuthRepository>();
      final password = _passwordCtrl.text;
      await auth.registerStaff(
        tenantCode: _tenantCtrl.text.trim(),
        email: _emailCtrl.text.trim(),
        phoneNumber: _phoneCtrl.text.trim(),
        password: password,
        displayName: _nameCtrl.text.trim(),
      );
      await context.read<SessionController>().markLoggedIn();
      if (!mounted) return;

      if (await _biometrics.isAvailable) {
        final save = await showDialog<bool>(
          context: context,
          builder: (ctx) => AlertDialog(
            title: Text(s.saveWithBiometrics),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(ctx, false),
                child: Text(s.saveBiometricsNo),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(ctx, true),
                child: Text(s.saveBiometricsYes),
              ),
            ],
          ),
        );
        if (save == true) {
          await _biometrics.save(
            tenantCode: _tenantCtrl.text.trim(),
            emailOrPhone: _emailCtrl.text.trim(),
            password: password,
          );
        }
      }

      if (!mounted) return;
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute<void>(builder: (_) => const HomeScreen()),
        (_) => false,
      );
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    return Scaffold(
      appBar: AppBar(title: Text(s.registerStaff)),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          TextField(
            controller: _tenantCtrl,
            textCapitalization: TextCapitalization.characters,
            decoration: InputDecoration(labelText: s.tenantCode),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _nameCtrl,
            decoration: InputDecoration(labelText: s.displayName),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _emailCtrl,
            keyboardType: TextInputType.emailAddress,
            decoration: InputDecoration(labelText: s.email),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _phoneCtrl,
            keyboardType: TextInputType.phone,
            decoration: InputDecoration(labelText: s.phone),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _passwordCtrl,
            obscureText: true,
            decoration: InputDecoration(labelText: s.password),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _confirmCtrl,
            obscureText: true,
            decoration: InputDecoration(labelText: s.confirmPassword),
          ),
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: TextStyle(color: Theme.of(context).colorScheme.error)),
          ],
          const SizedBox(height: 24),
          FilledButton(
            onPressed: _busy ? null : _submit,
            child: _busy
                ? const SizedBox(
                    height: 22,
                    width: 22,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                  )
                : Text(s.createAccount),
          ),
          TextButton(
            onPressed: _busy ? null : () => Navigator.of(context).pop(),
            child: Text(s.haveAccountSignIn),
          ),
        ],
      ),
    );
  }
}
