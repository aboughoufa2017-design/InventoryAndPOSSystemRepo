import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../auth/auth_repository.dart';
import '../auth/session_controller.dart';
import '../l10n/locale_controller.dart';
import 'home_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _tenantCodeController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _prefill());
  }

  Future<void> _prefill() async {
    final auth = context.read<AuthRepository>();
    final tenant = await auth.rememberedTenantCode;
    final email = await auth.rememberedEmail;
    if (!mounted) return;
    if (tenant != null && tenant.isNotEmpty) {
      _tenantCodeController.text = tenant;
    }
    if (email != null && email.isNotEmpty) {
      _emailController.text = email;
    }
  }

  @override
  void dispose() {
    _tenantCodeController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final auth = context.read<AuthRepository>();
      final session = context.read<SessionController>();
      await auth.login(
            tenantCode: _tenantCodeController.text.trim(),
            email: _emailController.text.trim(),
            password: _passwordController.text,
          );
      await session.markLoggedIn();
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
            controller: _emailController,
            keyboardType: TextInputType.emailAddress,
            autofillHints: const [AutofillHints.email],
            decoration: InputDecoration(
              labelText: s.email,
              prefixIcon: const Icon(Icons.mail_outline),
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
        ],
      ),
    );
  }
}
