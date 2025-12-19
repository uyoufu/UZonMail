---
title: Self-Hosted Postfix Mail Server
icon: server
order: 1
description: Using `uzoncloud.com` as an example, this guide explains how to build a production-grade Postfix mail server on low-spec VPS (DNS, MX/SPF/DMARC/DKIM, Postfix + Dovecot + OpenDKIM, SMTPS/STARTTLS, user management, PTR/RDNS) suitable for use with UzonMail for bulk sending.
permalink: /en/self-hosted/postfix
---

On low-end VPS (1 CPU, 1GB RAM, 20GB disk) heavy mail stacks like Mailcow are impractical. This guide focuses on a lightweight Postfix-based setup that can reliably send mail.

This tutorial uses the domain `uzoncloud.com` as an example. Adjust domain names and IPs to match your environment.

## Options comparison

Popular solutions include:

1. Mailcow
2. iRedMail
3. Postfix (manual)

Mailcow is a popular full-featured open-source mail stack built on Postfix. The first two solutions are heavier and require more resources. For low-spec machines Postfix is recommended.

## Recommended components

To minimize resource usage, a low-level implementation is most efficient. Recommended stack:

| # | Software | Purpose |
| - | -------- | ------- |
| 1 | Postfix  | SMTP server for sending/receiving mail |
| 2 | Dovecot  | IMAP/POP3 server for mailbox storage and client access |

## Overview of steps

1. Configure DNS: hostnames, MX, SPF, DMARC, DKIM, smtp/pop/imap host records.
2. Install and configure Postfix with TLS/STARTTLS.
3. Install and configure Dovecot for IMAP/POP and SMTP authentication.
4. Install OpenDKIM and generate DKIM keys.
5. Configure PTR/RDNS at your VPS provider.

### DNS configuration

Example DNS records:

| Type | Name | FQDN | Value | Purpose | Required |
| ---- | ---- | ---- | ----- | ------- | -------- |
| A    | mail1 | mail1.uzonmail.com | your server IP | host name and RDNS | Yes |
| MX   | @     | uzoncloud.com | mx.uzoncloud.com | mail exchange record | Yes |
| TXT  | @     | uzoncloud.com | v=spf1 a mx ip4:xx.xx.xx.xx ~all | SPF record to authorize sending IPs | Yes |
| TXT  | mail1 | mail1.uzonmail.com | v=spf1 redirect=uzoncloud.com | SPF redirect for host | Yes |
| TXT  | _dmarc | _dmarc.uzoncloud.com | v=DMARC1; p=quarantine; pct=100; adkim=r; aspf=r | DMARC policy and reporting | Yes |
| TXT  | default._domainkey | default._domainkey.uzoncloud.com | v=DKIM1; k=rsa; p=PUBLICKEY | DKIM public key | Yes |

Optional records (smtp/pop/imap hosts and alternate MX) can be added as needed.

Notes:

1. If `mx` is not set, ensure SPF references the correct host.
2. DKIM key pair is generated during the OpenDKIM setup step.
3. Adding `mail1` SPF reduces "SPF: HELO does not publish an SPF Record" warnings.

### Install Postfix

Install Postfix and choose "Internet Site" during setup:

``` bash
sudo apt update
sudo apt install -y postfix
```

Configure `main.cf` with minimum settings. Example commands:

``` bash
# set hostname
sudo postconf -e "myhostname = mail1.uzoncloud.com"
# store mail in Maildir
sudo postconf -e 'home_mailbox = Maildir/'
# accepted destinations
sudo postconf -e 'mydestination = $myhostname, localhost.$mydomain, localhost, uzoncloud.com, mx.uzoncloud.com'
# log file
sudo postconf -e 'maillog_file=/var/log/postfix.log'
# mailbox size (0 = unlimited)
sudo postconf -e 'mailbox_size_limit = 0'

# Enable SASL via Dovecot for SMTP AUTH
sudo postconf -e 'smtpd_sasl_type = dovecot'
sudo postconf -e 'smtpd_sasl_path = private/auth'
sudo postconf -e 'smtpd_sasl_local_domain = $myhostname'
sudo postconf -e 'smtpd_sasl_security_options = noanonymous,noplaintext'
sudo postconf -e 'smtpd_sasl_tls_security_options = noanonymous'
sudo postconf -e 'broken_sasl_auth_clients = yes'
sudo postconf -e 'smtpd_sasl_auth_enable = yes'
sudo postconf -e 'smtpd_recipient_restrictions = permit_sasl_authenticated,permit_mynetworks,reject_unauth_destination'

# TLS settings (use certificates from ACME or your provider)
sudo postconf -e 'smtp_tls_security_level = may'
sudo postconf -e 'smtpd_tls_security_level = may'
sudo postconf -e 'smtpd_tls_key_file = /etc/nginx/conf.d/cert/uzoncloud.com.key'
sudo postconf -e 'smtpd_tls_cert_file = /etc/nginx/conf.d/cert/uzoncloud.com.cert'
sudo postconf -e 'smtpd_tls_loglevel = 1'
sudo postconf -e 'smtpd_tls_received_header = yes'
sudo postconf -e 'smtpd_tls_auth_only = yes'
```

#### Configure SMTPS/Submission

Harden MUA (Mail User Agent) restrictions for submission ports (587/465). Edit `/etc/postfix/master.cf` and ensure `submission`/`submissions` entries provide STARTTLS/TLS and required security options. Add restrictions so only authenticated clients can submit mail.

Example additions include enforcing `smtpd_sasl_auth_enable=yes` and applying client/helo/sender/relay/recipient restrictions for MUAs.

#### Redirect system account mail (optional)

Edit `/etc/aliases` to redirect system mail for `root`/`postmaster` to your admin user to avoid mail being written to local system mailboxes.

### Install Dovecot

Install Dovecot for IMAP/POP3 and configure authentication for Postfix:

``` bash
sudo apt install -y dovecot-core dovecot-imapd dovecot-pop3d
```

Dovecot config files are in `/etc/dovecot/conf.d`.

Edit `10-auth.conf` to allow login mechanism `plain login`.

Edit `10-master.conf` to add the Postfix auth socket listener:

```
unix_listener /var/spool/postfix/private/auth {
  mode = 0660
  user = postfix
  group = postfix
}
```

Set mailbox location in `/etc/dovecot/dovecot.conf`:

```
mail_location = maildir:~/Maildir
```

Configure SSL in `10-ssl.conf` to point to your certificate files.

### Install OpenDKIM

OpenDKIM signs outgoing mail and verifies incoming signatures. Steps:

1. Install:

``` bash
sudo apt update
sudo apt install -y opendkim opendkim-tools
```

2. Generate DKIM keys:

``` bash
sudo mkdir -p /etc/opendkim/keys/uzoncloud.com
sudo opendkim-genkey -s default -d uzoncloud.com -D /etc/opendkim/keys/uzoncloud.com
sudo chown opendkim:opendkim /etc/opendkim/keys/uzoncloud.com/default.private
sudo chmod 600 /etc/opendkim/keys/uzoncloud.com/default.private
# view public key for DNS TXT record
cat /etc/opendkim/keys/uzoncloud.com/default.txt
```

3. Add the public key value to DNS as `default._domainkey.uzoncloud.com` (paste and remove surrounding quotes/newlines as instructed).

4. Configure `/etc/opendkim.conf` with `Domain`, `KeyFile`, `Selector`, and socket settings (e.g., `Socket inet:8891@localhost`).

5. Create KeyTable, SigningTable and TrustedHosts files:

``` bash
echo "default._domainkey.uzoncloud.com uzoncloud.com:default:/etc/opendkim/keys/uzoncloud.com/default.private" > /etc/opendkim/KeyTable
echo "*@uzoncloud.com default._domainkey.uzoncloud.com" > /etc/opendkim/SigningTable
echo -e "localhost
127.0.0.1
::1
uzoncloud.com" > /etc/opendkim/TrustedHosts
```

6. Validate config:

``` bash
sudo opendkim -n -c /etc/opendkim.conf
```

7. Integrate with Postfix: add milter settings to `/etc/postfix/main.cf`:

``` bash
milter_default_action = accept
smtpd_milters = inet:localhost:8891
non_smtpd_milters = $smtpd_milters
```

Optionally add per-service milter lines in `/etc/postfix/master.cf` for smtp/submission.

### User management

Start services and create mail users. Example:

``` bash
sudo systemctl start postfix dovecot opendkim
sudo systemctl enable postfix dovecot opendkim
```

Add mail users according to your chosen userdb (system users, virtual users, or other backends). Configure Dovecot accordingly.
