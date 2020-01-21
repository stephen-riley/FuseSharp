use strict;
use warnings;

use Fcntl 'SEEK_SET';

open my $fh, '<', '/tmp/fstest/subdir/file.txt' or die "Cannot open test file: $!\n";
sysseek( $fh, 10, 0 );
my $text = '\0' x 10;
sysread( $fh, $text, 10 );
print "$text\n";