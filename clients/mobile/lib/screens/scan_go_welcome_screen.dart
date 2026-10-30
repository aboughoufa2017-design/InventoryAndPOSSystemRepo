import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../main.dart';
import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import 'login_screen.dart';
import 'scan_go_cart_screen.dart';

/// Customer entry: continue as guest or sign in (staff).
class ScanGoWelcomeScreen extends StatefulWidget {
  const ScanGoWelcomeScreen({super.key});

  @override
  State<ScanGoWelcomeScreen> createState() => _ScanGoWelcomeScreenState();
}

class _ScanGoWelcomeScreenState extends State<ScanGoWelcomeScreen> {
  final _tenantCtrl = TextEditingController(text: '123');
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _tenantCtrl.dispose();
    super.dispose();
  }

  Future<void> _startGuest() async {
    final code = _tenantCtrl.text.trim();
    if (code.isEmpty) {
      setState(() => _error = 'Enter store / tenant code');
      return;
    }
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      final session = await api.startGuest(tenantCode: code);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ScanGoCartScreen(session: session),
        ),
      );
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [
              ModernTechColors.ice,
              Color(0xFFE8EEF8),
              ModernTechColors.white,
            ],
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(28),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Spacer(),
                Text(
                  'Modern Tech',
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.headlineLarge?.copyWith(
                        color: ModernTechColors.royalBlue,
                        fontWeight: FontWeight.w800,
                        letterSpacing: -0.5,
                      ),
                ),
                const SizedBox(height: 8),
                Text(
                  'مودرن تك',
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        color: ModernTechColors.electricBlue,
                      ),
                ),
                const SizedBox(height: 16),
                Text(
                  'Scan & Go — shop in-store, pay at the cashier.',
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                        color: ModernTechColors.muted,
                      ),
                ),
                const SizedBox(height: 36),
                TextField(
                  controller: _tenantCtrl,
                  textCapitalization: TextCapitalization.characters,
                  decoration: const InputDecoration(
                    labelText: 'Store code',
                    hintText: 'Tenant code',
                  ),
                ),
                if (_error != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    _error!,
                    style: TextStyle(color: Theme.of(context).colorScheme.error),
                  ),
                ],
                const SizedBox(height: 24),
                FilledButton(
                  onPressed: _busy ? null : _startGuest,
                  child: _busy
                      ? const SizedBox(
                          height: 22,
                          width: 22,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                        )
                      : const Text('Continue as Guest'),
                ),
                const SizedBox(height: 12),
                OutlinedButton(
                  onPressed: _busy
                      ? null
                      : () {
                          Navigator.of(context).push(
                            MaterialPageRoute<void>(
                              builder: (_) => const LoginScreen(),
                            ),
                          );
                        },
                  child: const Text('Staff sign in'),
                ),
                const Spacer(flex: 2),
                Text(
                  kApiBaseUrl,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: ModernTechColors.muted,
                      ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
