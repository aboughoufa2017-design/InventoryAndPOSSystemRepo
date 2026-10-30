import 'package:flutter/material.dart';

/// Modern Tech brand tokens from the POS specification (§5).
abstract final class ModernTechColors {
  static const Color royalBlue = Color(0xFF0A42A0);
  static const Color electricBlue = Color(0xFF0052FF);
  static const Color white = Color(0xFFFFFFFF);
  static const Color slate = Color(0xFF0F172A);
  static const Color ice = Color(0xFFF4F7FB);
  static const Color muted = Color(0xFF64748B);
}

ThemeData buildModernTechTheme() {
  final scheme = ColorScheme.fromSeed(
    seedColor: ModernTechColors.royalBlue,
    primary: ModernTechColors.royalBlue,
    secondary: ModernTechColors.electricBlue,
    surface: ModernTechColors.white,
    brightness: Brightness.light,
  );

  return ThemeData(
    useMaterial3: true,
    colorScheme: scheme,
    scaffoldBackgroundColor: ModernTechColors.ice,
    appBarTheme: const AppBarTheme(
      backgroundColor: ModernTechColors.royalBlue,
      foregroundColor: ModernTechColors.white,
      elevation: 0,
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: ModernTechColors.royalBlue,
        foregroundColor: ModernTechColors.white,
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: ModernTechColors.royalBlue,
        side: const BorderSide(color: ModernTechColors.royalBlue),
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: ModernTechColors.white,
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: ModernTechColors.electricBlue, width: 2),
      ),
    ),
  );
}
