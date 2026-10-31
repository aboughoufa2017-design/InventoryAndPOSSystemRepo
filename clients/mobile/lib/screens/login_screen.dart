import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../auth/auth_repository.dart';
import '../auth/biometric_credentials.dart';
import '../auth/session_controller.dart';
import '../l10n/locale_controller.dart';
import 'home_screen.dart';
import 'register_staff_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _tenantCodeController = TextEditingController();
  final _identifierController = TextEditingController();
  final _passwordController = TextEditingController();
  final _biometrics = BiometricCredentials();
  bool _busy = false;
  bool _bioAvailable = false;
  bool _hasBioCreds = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _bootstrap());
  }

  Future<void> _bootstrap() async {
    final auth = context.read<AuthRepository>();
    final tenant = await auth.rememberedTenantCode;
    final email = await auth.rememberedEmail;
    final bioOk = await _biometrics.isAvailable;
    final hasCreds = await _biometrics.hasSavedCredentials;
    if (!mounted) return;
    setState(() {
      if (tenant != null && tenant.isNotEmpty) {
        _tenantCodeController.text = tenant;
      }
      if (email != null && email.isNotEmpty) {
        _identifierController.text = email;
      }
      _bioAvailable = bioOk;
      _hasBioCreds = hasCreds;
    });
  }

  @override
  void dispose() {
    _tenantCodeController.dispose();
    _identifierController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _afterLoginSuccess(String password) async {
    final session = context.read<SessionController>();
    await session.markLoggedIn();
    if (!mounted) return;

    if (_bioAvailable) {
      final s = context.read<LocaleController>().s;
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
          tenantCode: _tenantCodeController.text.trim(),
          emailOrPhone: _identifierController.text.trim(),
          password: password,
        );
      }
    }

    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(builder: (_) => const HomeScreen()),
      (_) => false,
    );
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    final password = _passwordController.text;
    try {
      final auth = context.read<AuthRepository>();
      await auth.login(
        tenantCode: _tenantCodeController.text.trim(),
        emailOrPhone: _identifierController.text.trim(),
        password: password,
      );
      if (!mounted) return;
      await _afterLoginSuccess(password);
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _biometricLogin() async {
    final s = context.read<LocaleController>().s;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final unlocked = await _biometrics.unlock(reason: s.signInWithBiometrics);
      if (unlocked == null) {
        setState(() => _error = s.biometricsFailed);
        return;
      }
      _tenantCodeController.text = unlocked.tenantCode;
      _identifierController.text = unlocked.emailOrPhone;
      final auth = context.read<AuthRepository>();
      await auth.login(
        tenantCode: unlocked.tenantCode,
        emailOrPhone: unlocked.emailOrPhone,
        password: unlocked.password,
      );
      if (!mounted) return;
      await context.read<SessionController>().markLoggedIn();
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

  Future<void> _forgetBiometrics() async {
    await _biometrics.clear();
    if (mounted) setState(() => _hasBioCreds = false);
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    return Scaffold(
      appBar: AppBar(title: Text(s.staffLogin)),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          TextField(
            controller: _tenantCodeController,
            textCapitalization: TextCapitalization.characters,
            decoration: InputDecoration(
              labelText: s.tenantCode,
              prefixIcon: const Icon(Icons.apartment_outlined),
            ),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _identifierController,
            keyboardType: TextInputType.emailAddress,
            autofillHints: const [AutofillHints.username, AutofillHints.email],
            decoration: InputDecoration(
              labelText: s.emailOrPhone,
              prefixIcon: const Icon(Icons.person_outline),
            ),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _passwordController,
            obscureText: true,
            autofillHints: const [AutofillHints.password],
            decoration: InputDecoration(
              labelText: s.password,
              prefixIcon: const Icon(Icons.lock_outline),
            ),
            onSubmitted: (_) => _busy ? null : _submit(),
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
                : Text(s.signIn),
          ),
          if (_bioAvailable && _hasBioCreds) ...[
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: _busy ? null : _biometricLogin,
              icon: const Icon(Icons.fingerprint),
              label: Text(s.signInWithBiometrics),
            ),
            TextButton(
              onPressed: _busy ? null : _forgetBiometrics,
              child: Text(s.forgetSavedPassword),
            ),
          ],
          const SizedBox(height: 16),
          TextButton(
            onPressed: _busy
                ? null
                : () {
                    Navigator.of(context).push(
                      MaterialPageRoute<void>(builder: (_) => const RegisterStaffScreen()),
                    );
                  },
            child: Text(s.registerStaff),
          ),
        ],
      ),
    );
  }
}
